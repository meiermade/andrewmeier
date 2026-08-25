import { describe, expect, it } from 'vitest';
import { createConsentController, type AnalyticsMode, type ConsentChoice } from './consent';

function harness(options: {
  mode?: AnalyticsMode;
  saved?: ConsentChoice;
  persistenceError?: boolean;
  enableError?: boolean;
} = {}) {
  const events: string[] = [];
  const controller = createConsentController({
    policy: { analytics: options.mode ?? 'opt-in' },
    persistence: {
      readChoice: () => options.saved,
      persist: async choice => {
        events.push(`persist:${choice}`);
        if (options.persistenceError) throw new Error('unavailable');
      },
      clearChoice: () => events.push('clear-cookie'),
    },
    lifecycle: {
      enable: async () => {
        events.push('enable');
        if (options.enableError) throw new Error('partially activated');
      },
      disable: async () => { events.push('disable'); },
    },
    view: {
      setSaving: saving => events.push(`saving:${saving}`),
      hideError: () => events.push('hide-error'),
      showError: () => events.push('show-error'),
      hide: () => events.push('hide'),
      show: moveFocus => events.push(`show:${moveFocus}`),
    },
  });
  return { controller, events };
}

describe('analytics consent controller', () => {
  it('applies an existing server choice without persisting it again', async () => {
    const { controller, events } = harness({ saved: 'accepted' });

    await controller.initialize();

    expect(events).toEqual(['enable', 'hide']);
  });

  it('starts default-on analytics without creating an implicit choice', async () => {
    const { controller, events } = harness({ mode: 'default-on' });

    await controller.initialize();

    expect(events).toEqual(['enable', 'hide']);
  });

  it('fails closed and shows controls when the regional policy requires opt-in', async () => {
    const { controller, events } = harness({ mode: 'opt-in' });

    await controller.initialize();

    expect(events).toEqual(['disable', 'show:false']);
  });

  it('persists acceptance before enabling analytics', async () => {
    const { controller, events } = harness();

    await controller.setChoice('accepted');

    expect(events).toEqual([
      'saving:true', 'hide-error', 'persist:accepted',
      'enable', 'hide', 'saving:false',
    ]);
  });

  it('disables analytics before persisting withdrawal', async () => {
    const { controller, events } = harness();

    await controller.setChoice('declined');

    expect(events).toEqual([
      'saving:true', 'hide-error', 'disable', 'persist:declined',
      'hide', 'saving:false',
    ]);
  });

  it('opens settings with explicit focus movement', () => {
    const { controller, events } = harness();

    controller.showSettings();

    expect(events).toEqual(['show:true']);
  });

  it('rolls back partially activated analytics when enabling fails', async () => {
    const { controller, events } = harness({ enableError: true });

    await controller.setChoice('accepted');

    expect(events).toEqual([
      'saving:true', 'hide-error', 'persist:accepted', 'enable', 'disable',
      'clear-cookie', 'show-error', 'show:false', 'saving:false',
    ]);
  });

  it('fails closed when persistence fails', async () => {
    const { controller, events } = harness({ persistenceError: true });

    await controller.setChoice('accepted');

    expect(events).toEqual([
      'saving:true', 'hide-error', 'persist:accepted', 'disable', 'clear-cookie',
      'show-error', 'show:false', 'saving:false',
    ]);
  });
});
