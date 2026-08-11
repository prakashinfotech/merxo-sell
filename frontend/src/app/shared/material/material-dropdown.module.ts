import { NgModule } from '@angular/core';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatOptionModule } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';

/**
 * Material dropdown bundle.
 *
 * Re-exports the three modules every admin/seller feature module needs to
 * render `<mat-form-field><mat-select>…</mat-select></mat-form-field>` blocks.
 * Pulled into a tiny module so each feature gets one import line instead of
 * three, and so the styling lives in one well-known place.
 */
@NgModule({
  imports: [
    MatFormFieldModule,
    MatSelectModule,
    MatOptionModule,
    MatInputModule,
  ],
  exports: [
    MatFormFieldModule,
    MatSelectModule,
    MatOptionModule,
    MatInputModule,
  ],
})
export class MaterialDropdownModule {}
