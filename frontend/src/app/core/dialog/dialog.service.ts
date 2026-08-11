import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialog, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class DialogService {
  constructor(private dialog: MatDialog) {}

  confirm(options: ConfirmDialogData): Observable<boolean> {
    const dialogRef = this.dialog.open(ConfirmDialog, {
      width: '440px',
      data: options,
      panelClass: 'premium-dialog-panel'
    });

    return dialogRef.afterClosed();
  }

  /** Shorthand for simple confirmation */
  ask(titleOrOptions: string | ConfirmDialogData, message?: string, isDanger = false): Observable<boolean> {
    if (typeof titleOrOptions === 'string') {
      return this.confirm({
        title: titleOrOptions,
        message: message || '',
        isDanger,
        confirmText: isDanger ? 'Delete' : 'Confirm',
        cancelText: 'Cancel'
      });
    }
    return this.confirm(titleOrOptions);
  }

  /** Shorthand for non-interactive notifications */
  notify(title: string, message: string): void {
    this.dialog.open(ConfirmDialog, {
      width: '400px',
      data: {
        title,
        message,
        confirmText: 'OK',
        hideCancel: true
      },
      panelClass: 'premium-dialog-panel'
    });
  }
}
