import { context, trace, type Attributes } from '@opentelemetry/api';
import { logs, SeverityNumber, type LogRecord } from '@opentelemetry/api-logs';
import { ErrorsInstrumentation } from '@opentelemetry/browser-instrumentation/experimental/errors';
import { NavigationInstrumentation } from '@opentelemetry/browser-instrumentation/experimental/navigation';
import { WebVitalsInstrumentation } from '@opentelemetry/browser-instrumentation/experimental/web-vitals';
import { OTLPLogExporter } from '@opentelemetry/exporter-logs-otlp-http';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-http';
import { registerInstrumentations } from '@opentelemetry/instrumentation';
import { FetchInstrumentation } from '@opentelemetry/instrumentation-fetch';
import { resourceFromAttributes } from '@opentelemetry/resources';
import { BatchLogRecordProcessor, LoggerProvider } from '@opentelemetry/sdk-logs';
import { BatchSpanProcessor, WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { initializeConsent } from './consent';
import { consentChoice, pathAttributes, sanitizedUrl, trafficAttributes } from './event-contract';

const sessionKey = 'opentelemetry-session-id';
const attributionKey = 'opentelemetry-traffic-attribution';
const analyticsConsentWasPreviouslyGranted = consentChoice(document.cookie) === 'accepted';
const initialAttribution = trafficAttributes(window.location.href, document.referrer);
const script = document.getElementById('browser-telemetry') as HTMLScriptElement | null;
const endpoint = script?.dataset.otelEndpoint?.replace(/\/$/, '');
const completedArticles = new Set<string>();

let loggerProvider: LoggerProvider | undefined;
let tracerProvider: WebTracerProvider | undefined;
let deregisterInstrumentations: (() => void) | undefined;
let logger: ReturnType<typeof logs.getLogger> | undefined;
let initialization: Promise<void> | undefined;
let clickHandler: ((event: MouseEvent) => void) | undefined;
let lastTrackedPath: string | undefined;
let sessionAttribution: Attributes | undefined;

function sessionId(): string {
  const existing = sessionStorage.getItem(sessionKey);
  if (existing) return existing;

  const created = crypto.randomUUID();
  sessionStorage.setItem(sessionKey, created);
  return created;
}

function attributionAttributes(): Attributes {
  if (sessionAttribution) return sessionAttribution;

  const stored = sessionStorage.getItem(attributionKey);
  if (stored) {
    try {
      sessionAttribution = JSON.parse(stored) as Attributes;
      return sessionAttribution;
    } catch {
      sessionStorage.removeItem(attributionKey);
    }
  }

  sessionAttribution = initialAttribution;
  sessionStorage.setItem(attributionKey, JSON.stringify(sessionAttribution));
  return sessionAttribution;
}

function commonAttributes(): Attributes {
  return {
    ...attributionAttributes(),
    'session.id': sessionId(),
    'url.path': window.location.pathname,
  };
}

function addCommonEventData(record: LogRecord): void {
  const fullUrl = record.attributes?.['url.full'];
  record.attributes = {
    ...record.attributes,
    ...(typeof fullUrl === 'string' ? pathAttributes(fullUrl) : {}),
    ...commonAttributes(),
  };

  delete record.attributes['url.full'];
}

function emit(eventName: string, attributes: Attributes = {}): void {
  logger?.emit({
    eventName,
    severityNumber: SeverityNumber.INFO,
    attributes: { ...commonAttributes(), ...attributes },
  });
}

function currentArticleId(): string | undefined {
  const page = document.querySelector<HTMLElement>('[data-article-page]');
  return page?.dataset.telemetryContentId;
}

function trackArticleCompletion(): void {
  const id = currentArticleId();
  const article = document.querySelector<HTMLElement>('[data-article-page] article');
  if (!id || !article || completedArticles.has(id)) return;

  const articleBottom = article.getBoundingClientRect().bottom + window.scrollY;
  const maximumScroll = Math.max(articleBottom - window.innerHeight, 1);
  const progress = Math.min(Math.max(window.scrollY / maximumScroll, 0), 1);
  if (progress < 0.9) return;

  completedArticles.add(id);
  emit('com.meiermade.content.article_completed', {
    'com.meiermade.content.id': id,
    'com.meiermade.content.type': 'article',
  });
}

function trackPage(): void {
  if (!logger) return;

  const path = window.location.pathname;
  if (path === lastTrackedPath) return;
  lastTrackedPath = path;

  const id = currentArticleId();
  if (id) {
    emit('com.meiermade.content.article_opened', {
      'com.meiermade.content.id': id,
      'com.meiermade.content.type': 'article',
    });
    trackArticleCompletion();
  }
}

function onDocumentClick(event: MouseEvent): void {
  const anchor = event.target instanceof Element
    ? event.target.closest<HTMLAnchorElement>('a[href]')
    : null;
  if (!anchor) return;

  const destination = new URL(anchor.href, window.location.origin);
  if (!['http:', 'https:'].includes(destination.protocol) || destination.origin === window.location.origin) return;

  const isCompanySite = destination.hostname === 'meiermade.com' || destination.hostname.endsWith('.meiermade.com');
  emit(
    isCompanySite
      ? 'com.meiermade.marketing.company_site_clicked'
      : 'com.meiermade.content.outbound_link_clicked',
    { 'com.meiermade.link.domain': destination.hostname },
  );
}

function onPopState(): void {
  window.setTimeout(trackPage, 0);
}

function flushWhenHidden(): void {
  if (document.visibilityState !== 'hidden') return;
  void Promise.allSettled([
    loggerProvider?.forceFlush() ?? Promise.resolve(),
    tracerProvider?.forceFlush() ?? Promise.resolve(),
  ]);
}

async function initialize(): Promise<void> {
  if (!endpoint || loggerProvider || consentChoice(document.cookie) !== 'accepted') return;
  if (initialization) return initialization;

  initialization = (async () => {
    const resource = resourceFromAttributes({
      'service.name': 'andymeier-browser',
      'deployment.environment.name': 'production',
    });

    loggerProvider = new LoggerProvider({
      resource,
      logRecordLimits: { attributeCountLimit: 32, attributeValueLengthLimit: 256 },
      processors: [
        new BatchLogRecordProcessor({
          exporter: new OTLPLogExporter({ url: `${endpoint}/v1/logs` }),
          maxExportBatchSize: 32,
          maxQueueSize: 128,
          scheduledDelayMillis: 1000,
        }),
      ],
    });
    logs.setGlobalLoggerProvider(loggerProvider);

    tracerProvider = new WebTracerProvider({
      resource,
      spanLimits: { attributeCountLimit: 32, attributeValueLengthLimit: 256 },
      spanProcessors: [
        new BatchSpanProcessor(
          new OTLPTraceExporter({ url: `${endpoint}/v1/traces` }),
          {
            maxExportBatchSize: 32,
            maxQueueSize: 128,
            scheduledDelayMillis: 1000,
          },
        ),
      ],
    });
    tracerProvider.register();

    deregisterInstrumentations = registerInstrumentations({
      instrumentations: [
        new NavigationInstrumentation({
          sanitizeUrl: sanitizedUrl,
          applyCustomLogRecordData: addCommonEventData,
        }),
        ...(analyticsConsentWasPreviouslyGranted
          ? [new WebVitalsInstrumentation({
              includeRawAttribution: false,
              applyCustomLogRecordData: addCommonEventData,
            })]
          : []),
        new ErrorsInstrumentation({
          applyCustomAttributes: () => commonAttributes(),
        }),
        new FetchInstrumentation({
          ignoreUrls: [endpoint],
          propagateTraceHeaderCorsUrls: [window.location.origin],
          applyCustomAttributesOnSpan: (span, _request, result) => {
            if (result instanceof Response) {
              span.setAttribute('url.path', new URL(result.url).pathname);
            }
            for (const [key, value] of Object.entries(commonAttributes())) {
              if (value !== undefined && value !== null) span.setAttribute(key, value);
            }
          },
        }),
      ],
    });

    logger = logs.getLogger('com.meiermade.browser', '1.0.0');
    clickHandler = onDocumentClick;
    document.addEventListener('click', clickHandler, true);
    window.addEventListener('scroll', trackArticleCompletion, { passive: true });
    window.addEventListener('popstate', onPopState);
    document.addEventListener('visibilitychange', flushWhenHidden);
    trackPage();
  })().finally(() => {
    initialization = undefined;
  });

  return initialization;
}

async function stop(): Promise<void> {
  if (clickHandler) document.removeEventListener('click', clickHandler, true);
  clickHandler = undefined;
  window.removeEventListener('scroll', trackArticleCompletion);
  window.removeEventListener('popstate', onPopState);
  document.removeEventListener('visibilitychange', flushWhenHidden);
  deregisterInstrumentations?.();
  deregisterInstrumentations = undefined;

  await Promise.allSettled([
    loggerProvider?.shutdown() ?? Promise.resolve(),
    tracerProvider?.shutdown() ?? Promise.resolve(),
  ]);

  logger = undefined;
  loggerProvider = undefined;
  tracerProvider = undefined;
  lastTrackedPath = undefined;
  logs.disable();
  trace.disable();
  context.disable();
}

async function disable(): Promise<void> {
  await stop();
  sessionAttribution = undefined;
  sessionStorage.removeItem(sessionKey);
  sessionStorage.removeItem(attributionKey);
}

declare global {
  interface Window {
    meiermadeTelemetry: {
      emit: (eventName: string, attributes?: Attributes) => void;
      trackPage: () => void;
    };
  }
}

window.meiermadeTelemetry = { emit, trackPage };
initializeConsent({ enable: initialize, disable });
window.addEventListener('pagehide', (event) => {
  if (!event.persisted) void stop();
});
