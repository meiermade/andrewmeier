import type { Attributes } from '@opentelemetry/api';

export function sanitizedUrl(value: string): string {
  const url = new URL(value, window.location.origin);
  return `${url.origin}${url.pathname}`;
}

export function pathAttributes(value: string): Attributes {
  return { 'url.path': new URL(value, window.location.origin).pathname };
}

export type ConsentChoice = 'accepted' | 'declined';

const campaignValue = /^[a-z0-9][a-z0-9_-]{0,63}$/;

export function consentChoice(cookieHeader: string): ConsentChoice | undefined {
  const cookie = cookieHeader
    .split(';')
    .map(value => value.trim())
    .find(value => value.startsWith('analytics-consent='));
  if (!cookie) return undefined;

  const value = decodeURIComponent(cookie.slice('analytics-consent='.length));
  const match = /^v1\.(accepted|declined)\.\d{4}-\d{2}-\d{2}\.\d+$/.exec(value);
  return match?.[1] as ConsentChoice | undefined;
}

function normalizedSource(value: string): string {
  switch (value.trim().toLowerCase()) {
    case 'linkedin':
      return 'linkedin';
    case 'x':
    case 'twitter':
      return 'x';
    case 'google':
      return 'google';
    case 'bing':
      return 'bing';
    case 'email':
    case 'newsletter':
      return 'newsletter';
    default:
      return 'other';
  }
}

function inferredMedium(source: string): string {
  switch (source) {
    case 'linkedin':
    case 'x':
      return 'organic-social';
    case 'google':
    case 'bing':
      return 'organic-search';
    case 'newsletter':
      return 'email';
    default:
      return 'other';
  }
}

function normalizedMedium(value: string | null, source: string): string {
  if (!value) return inferredMedium(source);

  switch (value.trim().toLowerCase().replaceAll('_', '-')) {
    case 'organic-social':
    case 'social':
      return 'organic-social';
    case 'organic-search':
      return 'organic-search';
    case 'email':
      return 'email';
    case 'referral':
      return 'referral';
    case 'direct':
      return 'direct';
    default:
      return 'other';
  }
}

function referrerAttribution(referrer: string): [string, string] {
  if (!referrer) return ['direct', 'direct'];

  try {
    const hostname = new URL(referrer).hostname.toLowerCase();
    if (hostname === 'linkedin.com' || hostname.endsWith('.linkedin.com') || hostname === 'lnkd.in') {
      return ['linkedin', 'organic-social'];
    }
    if (hostname === 'x.com' || hostname.endsWith('.x.com') || hostname === 'twitter.com' || hostname.endsWith('.twitter.com') || hostname === 't.co') {
      return ['x', 'organic-social'];
    }
    if (/(^|[.])google[.][a-z.]+$/.test(hostname)) return ['google', 'organic-search'];
    if (hostname === 'bing.com' || hostname.endsWith('.bing.com')) return ['bing', 'organic-search'];
    return ['referral', 'referral'];
  } catch {
    return ['referral', 'referral'];
  }
}

export function trafficAttributes(urlValue: string, referrer: string): Attributes {
  const url = new URL(urlValue, window.location.origin);
  const sourceValue = url.searchParams.get('utm_source');
  const [source, medium] = sourceValue
    ? (() => {
        const normalized = normalizedSource(sourceValue);
        return [normalized, normalizedMedium(url.searchParams.get('utm_medium'), normalized)];
      })()
    : referrerAttribution(referrer);

  const attributes: Attributes = {
    'com.meiermade.traffic.source': source,
    'com.meiermade.traffic.medium': medium,
  };
  const campaign = url.searchParams.get('utm_campaign')?.trim().toLowerCase();
  const content = url.searchParams.get('utm_content')?.trim().toLowerCase();
  if (campaign && campaignValue.test(campaign)) attributes['com.meiermade.campaign.id'] = campaign;
  if (content && campaignValue.test(content)) attributes['com.meiermade.campaign.content'] = content;
  return attributes;
}
