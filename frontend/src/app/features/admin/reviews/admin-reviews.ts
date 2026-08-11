import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { PortalReviewDto, ReviewService } from '../../../core/review/review.service';

@Component({
  selector: 'app-admin-reviews',
  standalone: false,
  templateUrl: './admin-reviews.html',
  styleUrls: ['./admin-reviews.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminReviews implements OnInit {
  reviews: PortalReviewDto[] = [];
  loading = true;
  error = '';
  searchTerm = '';
  ratingFilter: number | null = null;
  readonly pageSize = 8;
  page = 1;

  constructor(private reviewsService: ReviewService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.reviewsService.getAdminReviews(this.searchTerm.trim() || undefined, this.ratingFilter || undefined).subscribe({
      next: reviews => {
        this.reviews = reviews;
        this.page = 1;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.error = 'Failed to load reviews.';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  get totalPages(): number { return Math.max(1, Math.ceil(this.reviews.length / this.pageSize)); }
  get pagedReviews(): PortalReviewDto[] {
    const start = (this.page - 1) * this.pageSize;
    return this.reviews.slice(start, start + this.pageSize);
  }
  goPage(page: number): void {
    this.page = Math.min(Math.max(page, 1), this.totalPages);
    this.cdr.markForCheck();
  }
  resetFilters(): void {
    this.searchTerm = '';
    this.ratingFilter = null;
    this.load();
  }
}
