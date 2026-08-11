import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-skeleton-card',
  templateUrl: './skeleton-card.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SkeletonCard {}
