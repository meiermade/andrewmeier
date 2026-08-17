import { consentChoice, type ConsentChoice } from './event-contract';

export type { ConsentChoice } from './event-contract';

interface ConsentPersistence {
  readChoice: () => ConsentChoice | undefined;
  readLegacyChoice: () => ConsentChoice | undefined;
  persist: (choice: ConsentChoice) => Promise<void>;
  clearLegacyChoice: () => void;
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
  show: () => void;
}

export function createConsentController(dependencies: {
  persistence: ConsentPersistence;
  lifecycle: AnalyticsLifecycle;
  view: ConsentView;
}) {
  const { persistence, lifecycle, view } = dependencies;

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
      persistence.clearLegacyChoice();
      if (choice === 'accepted') await lifecycle.enable();
      view.hide();
    } catch {
      persistence.clearChoice();
      view.showError();
      view.show();
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

    const legacy = persistence.readLegacyChoice();
    if (legacy) await setChoice(legacy);
    else view.show();
  }

  return { initialize, setChoice, showSettings: view.show };
}

function legacyChoice(): ConsentChoice | undefined {
  const value = localStorage.getItem('analytics-consent');
  return value === 'accepted' || value === 'declined' ? value : undefined;
}

export function initializeConsent(lifecycle: AnalyticsLifecycle): void {
  const start = () => {
    const banner = document.getElementById('cookie-consent-banner');
    if (!banner) return;

    const error = document.getElementById('analytics-consent-error');
    const buttons = Array.from(banner.querySelectorAll<HTMLButtonElement>('button'));
    const controller = createConsentController({
      persistence: {
        readChoice: () => consentChoice(document.cookie),
        readLegacyChoice: legacyChoice,
        persist: async choice => {
          const response = await fetch('/privacy/consent', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ analytics: choice }),
          });
          if (!response.ok) throw new Error('Unable to save analytics preference.');
        },
        clearLegacyChoice: () => localStorage.removeItem('analytics-consent'),
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
        hide: () => banner.classList.add('hidden'),
        show: () => banner.classList.remove('hidden'),
      },
    });

    document.getElementById('analytics-accept')?.addEventListener('click', () => {
      void controller.setChoice('accepted');
    });
    document.getElementById('analytics-reject')?.addEventListener('click', () => {
      void controller.setChoice('declined');
    });
    document.getElementById('analytics-settings')?.addEventListener('click', controller.showSettings);
    void controller.initialize();
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start, { once: true });
  else start();
}
