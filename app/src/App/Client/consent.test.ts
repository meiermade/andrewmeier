import { describe, expect, it } from 'vitest';
import { createConsentController, type ConsentChoice } from './consent';

function harness(options: {
  saved?: ConsentChoice;
  legacy?: ConsentChoice;
  persistenceError?: boolean;
} = {}) {
  const events: string[] = [];
  const controller = createConsentController({
    persistence: {
      readChoice: () => options.saved,
      readLegacyChoice: () => options.legacy,
      persist: async choice => {
        events.push(`persist:${choice}`);
        if (options.persistenceError) throw new Error('unavailable');
      },
      clearLegacyChoice: () => events.push('clear-legacy'),
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
      'saving:true', 'hide-error', 'persist:accepted', 'clear-legacy',
      'enable', 'hide', 'saving:false',
    ]);
  });

  it('disables analytics before persisting withdrawal', async () => {
    const { controller, events } = harness();

    await controller.setChoice('declined');

    expect(events).toEqual([
      'saving:true', 'hide-error', 'disable', 'persist:declined',
      'clear-legacy', 'hide', 'saving:false',
    ]);
  });

  it('migrates a legacy choice through the server and fails closed', async () => {
    const migrated = harness({ legacy: 'accepted' });
    await migrated.controller.initialize();
    expect(migrated.events).toContain('persist:accepted');
    expect(migrated.events).toContain('clear-legacy');

    const failed = harness({ persistenceError: true });
    await failed.controller.setChoice('accepted');
    expect(failed.events).toEqual([
      'saving:true', 'hide-error', 'persist:accepted', 'clear-cookie',
      'show-error', 'show', 'saving:false',
    ]);
  });
});
