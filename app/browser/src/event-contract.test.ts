import { describe, expect, it, vi } from 'vitest';
import { pathAttributes, sanitizedUrl } from './event-contract';

vi.stubGlobal('window', { location: { origin: 'https://andymeier.dev' } });

describe('browser telemetry event contract', () => {
  it('removes query strings and fragments from URLs', () => {
    expect(sanitizedUrl('https://andymeier.dev/articles/example?email=a%40example.com#private')).toBe(
      'https://andymeier.dev/articles/example',
    );
    expect(pathAttributes('/articles?token=secret')).toEqual({ 'url.path': '/articles' });
  });
});
