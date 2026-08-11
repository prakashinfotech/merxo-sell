import {
  Component, Input, OnInit, OnDestroy,
  ChangeDetectionStrategy, ChangeDetectorRef,
} from '@angular/core';
import { Router } from '@angular/router';
import { OfferBannerDto } from '../../../../core/banner/banner.service';

@Component({
  selector: 'app-mid-promo-banner',
  templateUrl: './mid-promo-banner.html',
  styleUrls: ['./mid-promo-banner.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MidPromoBanner implements OnInit, OnDestroy {
  @Input() banners: OfferBannerDto[] = [];
  @Input() align: 'left' | 'right' = 'left';

  @Input() set banner(val: OfferBannerDto | undefined) {
    if (val && (!this.banners || this.banners.length === 0)) {
      this.banners = [val];
    }
  }

  activeIndex = 0;
  private timer: number | null = null;
  private readonly INTERVAL_MS = 5000;

  constructor(private router: Router, private cdr: ChangeDetectorRef) {}

  get activeBanner(): OfferBannerDto | null {
    if (!this.banners || this.banners.length === 0) return null;
    return this.banners[this.activeIndex % this.banners.length] ?? null;
  }

  ngOnInit(): void {
    this.startCarousel();
  }

  ngOnDestroy(): void {
    this.stopCarousel();
  }

  goTo(index: number, event?: Event): void {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    this.activeIndex = index;
    this.restartCarousel();
    this.cdr.markForCheck();
  }

  pauseCarousel(): void {
    this.stopCarousel();
  }

  resumeCarousel(): void {
    this.startCarousel();
  }

  navigate(): void {
    const current = this.activeBanner;
    if (!current) return;
    if (current.linkedProductId) {
      this.router.navigate(['/products', current.linkedProductId]);
    } else if (current.linkedCategoryId) {
      this.router.navigate(['/products'], {
        queryParams: { categoryId: current.linkedCategoryId },
      });
    } else if (current.ctaUrl) {
      if (current.ctaUrl.startsWith('/')) {
        this.router.navigateByUrl(current.ctaUrl);
      } else {
        window.open(current.ctaUrl, '_blank', 'noopener');
      }
    }
  }

  onImgError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.onerror = null;
      img.src = 'http://localhost:5000/uploads/sample-mid-left-banner.jpg';
    }
  }

  private startCarousel(): void {
    if (this.timer !== null || !this.banners || this.banners.length <= 1) return;
    this.timer = window.setInterval(() => {
      this.activeIndex = (this.activeIndex + 1) % this.banners.length;
      this.cdr.markForCheck();
    }, this.INTERVAL_MS);
  }

  private stopCarousel(): void {
    if (this.timer !== null) {
      clearInterval(this.timer);
      this.timer = null;
    }
  }

  private restartCarousel(): void {
    this.stopCarousel();
    this.startCarousel();
  }
}
