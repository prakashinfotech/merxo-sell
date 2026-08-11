import { NgModule } from '@angular/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

/**
 * Material datepicker bundle.
 * Import this in any feature module that needs a date picker.
 *
 * Usage:
 *   <mat-form-field class="app-datepicker">
 *     <mat-label>Date</mat-label>
 *     <input matInput [matDatepicker]="dp" [(ngModel)]="date" />
 *     <mat-datepicker-toggle matIconSuffix [for]="dp"></mat-datepicker-toggle>
 *     <mat-datepicker #dp></mat-datepicker>
 *   </mat-form-field>
 */
@NgModule({
  imports: [
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  exports: [
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
  ],
})
export class MaterialDatepickerModule {}
