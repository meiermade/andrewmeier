import { describe, expect, it, vi } from 'vitest';
import { consentChoice, pathAttributes, sanitizedUrl, trafficAttributes } from './event-contract';

vi.stubGlobal('window', { location: { origin: 'https://andymeier.dev' } });

describe('browser telemetry event contract', () => {
  it('removes query strings and fragments from URLs', () => {
    expect(sanitizedUrl('https://andymeier.dev/articles/example?email=a%40example.com#private')).toBe(
      'https://andymeier.dev/articles/example',
    );
    expect(pathAttributes('/articles?token=secret')).toEqual({ 'url.path': '/articles' });
  });

  it('reads only versioned server consent cookies', () => {
    expect(consentChoice('theme=dark; analytics-consent=v1.accepted.2026-08-16.1786872000')).toBe('accepted');
    expect(consentChoice('analytics-consent=v1.declined.2026-08-16.1786872000')).toBe('declined');
    expect(consentChoice('analytics-consent=accepted')).toBeUndefined();
  });

  it('turns campaign parameters into bounded attribution attributes', () => {
    expect(trafficAttributes(
      'https://andymeier.dev/articles/example?utm_source=LinkedIn&utm_medium=organic-social&utm_campaign=otel-platform&utm_content=post-01&email=private%40example.com',
      '',
    )).toEqual({
      'com.meiermade.traffic.source': 'linkedin',
      'com.meiermade.traffic.medium': 'organic-social',
      'com.meiermade.campaign.id': 'otel-platform',
      'com.meiermade.campaign.content': 'post-01',
    });
  });

  it('uses a coarse referrer source without retaining the referrer URL', () => {
    expect(trafficAttributes('https://andymeier.dev/articles/example', 'https://t.co/private-path?token=secret')).toEqual({
      'com.meiermade.traffic.source': 'x',
      'com.meiermade.traffic.medium': 'organic-social',
    });
    expect(trafficAttributes('https://andymeier.dev/articles/example', '')).toEqual({
      'com.meiermade.traffic.source': 'direct',
      'com.meiermade.traffic.medium': 'direct',
    });
  });

  it('drops invalid campaign values instead of exporting arbitrary query content', () => {
    expect(trafficAttributes(
      'https://andymeier.dev/articles/example?utm_source=unknown-network&utm_campaign=private%40example.com&utm_content=' + 'x'.repeat(80),
      '',
    )).toEqual({
      'com.meiermade.traffic.source': 'other',
      'com.meiermade.traffic.medium': 'other',
    });
  });
});
