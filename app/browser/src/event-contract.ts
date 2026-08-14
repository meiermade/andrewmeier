import type { Attributes } from '@opentelemetry/api';

export function sanitizedUrl(value: string): string {
  const url = new URL(value, window.location.origin);
  return `${url.origin}${url.pathname}`;
}

export function pathAttributes(value: string): Attributes {
  return { 'url.path': new URL(value, window.location.origin).pathname };
}
