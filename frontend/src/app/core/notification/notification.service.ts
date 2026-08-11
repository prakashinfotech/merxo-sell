import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ToastService } from '../toast/toast.service';
import { DialogService } from '../dialog/dialog.service';
import { ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog';

/**
 * Unified notification facade.
 * Use this instead of injecting ToastService + DialogService separately.
 *
 * Toasts  → success / error / info / warn
 * Dialogs → confirm / ask / alert
 */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  constructor(
    private toast: ToastService,
    private dialog: DialogService
  ) {}

  // ── Toast shortcuts ─────────────────────────────────────────────────────────

  success(title: string, message?: string, durationMs = 4000): number {
    return this.toast.success(title, message, durationMs);
  }

  error(title: string, message?: string, durationMs = 6000): number {
    return this.toast.error(title, message, durationMs);
  }

  info(title: string, message?: string, durationMs = 4000): number {
    return this.toast.info(title, message, durationMs);
  }

  warn(title: string, message?: string, durationMs = 5000): number {
    return this.toast.warn(title, message, durationMs);
  }

  dismissToast(id: number): void {
    this.toast.dismiss(id);
  }

  clearAll(): void {
    this.toast.clear();
  }

  // ── Dialog shortcuts ────────────────────────────────────────────────────────

  confirm(options: ConfirmDialogData): Observable<boolean> {
    return this.dialog.confirm(options);
  }

  /** Simple yes/no dialog. Returns Observable<boolean>. */
  ask(title: string, message: string, isDanger = false): Observable<boolean> {
    return this.dialog.ask(title, message, isDanger);
  }

  /** Danger confirmation shorthand (red confirm button). */
  danger(title: string, message: string, confirmText = 'Delete'): Observable<boolean> {
    return this.dialog.confirm({ title, message, isDanger: true, confirmText, cancelText: 'Cancel' });
  }

  /** Non-interactive alert dialog (OK only). */
  alert(title: string, message: string): void {
    this.dialog.notify(title, message);
  }
}
