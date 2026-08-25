import type { Attributes } from '@opentelemetry/api';
import { initializeConsent, type ConsentPolicy } from './consent';
import { consentChoice, trafficAttributes } from './event-contract';

interface TelemetryModule {
  disableTelemetry: () => Promise<void>;
  enableTelemetry: (options: {
    endpoint: string;
    includeWebVitals: boolean;
    initialAttribution: Attributes;
  }) => Promise<void>;
}

const sessionKey = 'opentelemetry-session-id';
const attributionKey = 'opentelemetry-traffic-attribution';
const script = document.getElementById('privacy-controls') as HTMLScriptElement | null;
const endpoint = script?.dataset.otelEndpoint?.replace(/\/$/, '');
const telemetrySource = script?.dataset.telemetrySrc;
const initialAttribution = trafficAttributes(window.location.href, document.referrer);

function policyFromScript(): ConsentPolicy {
  return {
    analytics: script?.dataset.analyticsMode === 'default-on' ? 'default-on' : 'opt-in',
  };
}

const policy = policyFromScript();
const savedChoice = consentChoice(document.cookie);
const includeWebVitals = savedChoice === 'accepted'
  || (savedChoice === undefined && policy.analytics === 'default-on');
let telemetryModule: Promise<TelemetryModule> | undefined;

function loadTelemetry(): Promise<TelemetryModule> {
  if (!telemetrySource) return Promise.reject(new Error('Browser telemetry is unavailable.'));
  if (!telemetryModule) {
    telemetryModule = import(telemetrySource)
      .then(module => module as TelemetryModule)
      .catch(error => {
        telemetryModule = undefined;
        throw error;
      });
  }
  return telemetryModule;
}

initializeConsent(policy, {
  enable: async () => {
    if (!endpoint || !telemetrySource) return;
    const telemetry = await loadTelemetry();
    await telemetry.enableTelemetry({ endpoint, includeWebVitals, initialAttribution });
  },
  disable: async () => {
    sessionStorage.removeItem(sessionKey);
    sessionStorage.removeItem(attributionKey);
    if (!telemetryModule) return;
    const telemetry = await telemetryModule;
    await telemetry.disableTelemetry();
  },
});
