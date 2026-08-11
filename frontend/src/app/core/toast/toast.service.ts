import { Injectable, signal } from '@angular/core';

export type ToastVariant = 'success' | 'error' | 'info' | 'warn';

export interface ToastEntry {
  id: number;
  variant: ToastVariant;
  title: string;
  message?: string;
  /** Auto-dismiss timeout (ms). 0 disables auto-dismiss. */
  durationMs: number;
  /** Internal — flips true while the leave animation plays. */
  leaving?: boolean;
}

/**
 * App-wide toast notification feed. Producers call `success/error/info/warn`;
 * the global <app-toast-host> in app.html subscribes via the signal and
 * renders the stack with the design defined in styles.scss (.toast / .toast--*).
 *
 * Toasts auto-dismiss after `durationMs`; the user can close earlier with the
 * × button which triggers the leave animation before removal.
 */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  private readonly _toasts = signal<ToastEntry[]>([]);
  readonly toasts = this._toasts.asReadonly();

  success(title: string, message?: string, durationMs = 4000): number {
    return this.push('success', title, message, durationMs);
  }

  error(title: string, message?: string, durationMs = 6000): number {
    return this.push('error', title, message, durationMs);
  }

  info(title: string, message?: string, durationMs = 4000): number {
    return this.push('info', title, message, durationMs);
  }

  warn(title: string, message?: string, durationMs = 5000): number {
    return this.push('warn', title, message, durationMs);
  }

  dismiss(id: number): void {
    // Two-phase removal so the slide-out animation has time to play.
    const current = this._toasts();
    const idx = current.findIndex(t => t.id === id);
    if (idx === -1) return;
    const next = [...current];
    next[idx] = { ...next[idx], leaving: true };
    this._toasts.set(next);
    setTimeout(() => {
      this._toasts.set(this._toasts().filter(t => t.id !== id));
    }, 180);
  }

  clear(): void { this._toasts.set([]); }

  private push(variant: ToastVariant, title: string, message: string | undefined, durationMs: number): number {
    const id = this.nextId++;
    this._toasts.set([...this._toasts(), { id, variant, title, message, durationMs }]);
    if (durationMs > 0) {
      setTimeout(() => this.dismiss(id), durationMs);
    }
    return id;
  }
}
