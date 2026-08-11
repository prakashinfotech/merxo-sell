import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ModerationService, FlaggedReviewDto } from '../../../core/admin/moderation.service';

@Component({
  selector: 'app-moderation-reviews',
  templateUrl: './moderation-reviews.html',
  styleUrls: ['./moderation.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ModerationReviews implements OnInit {
  loading = true;
  saving  = false;
  error   = '';
  reviews: FlaggedReviewDto[] = [];
  rejectingId: number | null = null;
  rejectReason = '';

  constructor(private svc: ModerationService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.cdr.markForCheck();
    this.svc.getFlaggedReviews().subscribe({
      next: rows => { this.reviews = rows; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.error = 'Failed to load flagged reviews.'; this.loading = false; this.cdr.markForCheck(); },
    });
  }

  approve(r: FlaggedReviewDto): void {
    this.saving = true;
    this.cdr.markForCheck();
    this.svc.approveReview(r.reviewId).subscribe({
      next: () => {
        this.reviews = this.reviews.filter(x => x.reviewId !== r.reviewId);
        this.saving = false; this.cdr.markForCheck();
      },
      error: () => { this.saving = false; this.error = 'Approve failed.'; this.cdr.markForCheck(); },
    });
  }

  beginReject(id: number): void { this.rejectingId = id; this.rejectReason = ''; this.cdr.markForCheck(); }
  cancelReject(): void { this.rejectingId = null; this.rejectReason = ''; this.cdr.markForCheck(); }

  confirmReject(): void {
    if (!this.rejectingId || !this.rejectReason.trim()) return;
    const id = this.rejectingId;
    this.saving = true;
    this.cdr.markForCheck();
    this.svc.rejectReview(id, this.rejectReason.trim()).subscribe({
      next: () => {
        this.reviews = this.reviews.filter(x => x.reviewId !== id);
        this.cancelReject();
        this.saving = false; this.cdr.markForCheck();
      },
      error: () => { this.saving = false; this.error = 'Reject failed.'; this.cdr.markForCheck(); },
    });
  }

  stars(n: number): number[] { return Array.from({ length: n }, (_, i) => i); }
}
