import { describe, expect, it } from 'vitest';
import { createConsentController, type ConsentChoice } from './consent';

function harness(options: {
  saved?: ConsentChoice;
  persistenceError?: boolean;
} = {}) {
  const events: string[] = [];
  const controller = createConsentController({
    persistence: {
      readChoice: () => options.saved,
      persist: async choice => {
        events.push(`persist:${choice}`);
        if (options.persistenceError) throw new Error('unavailable');
      },
      clearChoice: () => events.push('clear-cookie'),
    },
    lifecycle: {
      enable: async () => { events.push('enable'); },
      disable: async () => { events.push('disable'); },
    },
    view: {
      setSaving: saving => events.push(`saving:${saving}`),
      hideError: () => events.push('hide-error'),
      showError: () => events.push('show-error'),
      hide: () => events.push('hide'),
      show: () => events.push('show'),
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

  it('shows the banner when no server choice exists', async () => {
    const { controller, events } = harness();

    await controller.initialize();

    expect(events).toEqual(['show']);
  });

  it('fails closed when persistence fails', async () => {
    const { controller, events } = harness({ persistenceError: true });

    await controller.setChoice('accepted');

    expect(events).toEqual([
      'saving:true', 'hide-error', 'persist:accepted', 'clear-cookie',
      'show-error', 'show', 'saving:false',
    ]);
  });
});
