import { consentChoice, type ConsentChoice } from './event-contract';

export type { ConsentChoice } from './event-contract';

export type AnalyticsMode = 'default-on' | 'opt-in';
export interface ConsentPolicy {
  analytics: AnalyticsMode;
}

interface ConsentPersistence {
  readChoice: () => ConsentChoice | undefined;
  persist: (choice: ConsentChoice) => Promise<void>;
  clearChoice: () => void;
}

interface AnalyticsLifecycle {
  enable: () => Promise<void>;
  disable: () => Promise<void>;
}

interface ConsentView {
  setSaving: (saving: boolean) => void;
  hideError: () => void;
  showError: () => void;
  hide: () => void;
  show: (moveFocus: boolean) => void;
}

export function createConsentController(dependencies: {
  policy: ConsentPolicy;
  persistence: ConsentPersistence;
  lifecycle: AnalyticsLifecycle;
  view: ConsentView;
}) {
  const { policy, persistence, lifecycle, view } = dependencies;

  async function apply(choice: ConsentChoice): Promise<void> {
    if (choice === 'accepted') await lifecycle.enable();
    else await lifecycle.disable();
  }

  async function setChoice(choice: ConsentChoice): Promise<void> {
    view.setSaving(true);
    view.hideError();

    try {
      if (choice === 'declined') await lifecycle.disable();
      await persistence.persist(choice);
      if (choice === 'accepted') await lifecycle.enable();
      view.hide();
    } catch {
      await lifecycle.disable().catch(() => undefined);
      persistence.clearChoice();
      view.showError();
      view.show(false);
    } finally {
      view.setSaving(false);
    }
  }

  async function initialize(): Promise<void> {
    const saved = persistence.readChoice();
    if (saved) {
      await apply(saved);
      view.hide();
      return;
    }

    if (policy.analytics === 'default-on') {
      await lifecycle.enable();
      view.hide();
      return;
    }

    await lifecycle.disable();
    view.show(false);
  }

  return { initialize, setChoice, showSettings: () => view.show(true) };
}

export function initializeConsent(policy: ConsentPolicy, lifecycle: AnalyticsLifecycle): void {
  const start = () => {
    const banner = document.getElementById('cookie-consent-banner');
    if (!banner) return;

    const error = document.getElementById('analytics-consent-error');
    const title = document.getElementById('analytics-consent-title');
    const settings = document.getElementById('analytics-settings');
    const buttons = Array.from(banner.querySelectorAll<HTMLButtonElement>('button'));
    let returnFocus: HTMLElement | null = null;
    const controller = createConsentController({
      policy,
      persistence: {
        readChoice: () => consentChoice(document.cookie),
        persist: async choice => {
          const response = await fetch('/privacy/consent', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ analytics: choice }),
          });
          if (!response.ok) throw new Error('Unable to save analytics preference.');
        },
        clearChoice: () => {
          document.cookie = 'analytics-consent=; Max-Age=0; Path=/; SameSite=Lax';
        },
      },
      lifecycle,
      view: {
        setSaving: saving => buttons.forEach(button => { button.disabled = saving; }),
        hideError: () => error?.classList.add('hidden'),
        showError: () => {
          if (!error) return;
          error.textContent = 'We could not save your analytics preference. Please try again.';
          error.classList.remove('hidden');
        },
        hide: () => {
          banner.classList.add('hidden');
          settings?.setAttribute('aria-expanded', 'false');
          if (returnFocus?.isConnected) returnFocus.focus();
          returnFocus = null;
        },
        show: moveFocus => {
          if (moveFocus && document.activeElement instanceof HTMLElement) returnFocus = document.activeElement;
          banner.classList.remove('hidden');
          settings?.setAttribute('aria-expanded', 'true');
          if (moveFocus) requestAnimationFrame(() => title?.focus());
        },
      },
    });

    document.getElementById('analytics-accept')?.addEventListener('click', () => {
      void controller.setChoice('accepted');
    });
    document.getElementById('analytics-reject')?.addEventListener('click', () => {
      void controller.setChoice('declined');
    });
    settings?.addEventListener('click', controller.showSettings);
    void controller.initialize();
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start, { once: true });
  else start();
}
