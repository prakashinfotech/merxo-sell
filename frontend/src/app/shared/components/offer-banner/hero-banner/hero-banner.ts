import {
  Component, Input, OnInit, OnDestroy,
  ChangeDetectionStrategy, ChangeDetectorRef,
} from '@angular/core';
import { Router } from '@angular/router';
import { OfferBannerDto } from '../../../../core/banner/banner.service';

@Component({
  selector: 'app-hero-banner',
  templateUrl: './hero-banner.html',
  styleUrls: ['./hero-banner.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeroBanner implements OnInit, OnDestroy {
  @Input() banners: OfferBannerDto[] = [];

  activeIndex = 0;
  loading = true;
  private timer: number | null = null;
  private readonly INTERVAL_MS = 6000;

  constructor(private router: Router, private cdr: ChangeDetectorRef) {}

  get active(): OfferBannerDto | null {
    return this.banners[this.activeIndex] ?? null;
  }

  ngOnInit(): void {
    this.loading = false;
    this.startCarousel();
  }

  ngOnDestroy(): void {
    this.stopCarousel();
  }

  goTo(index: number): void {
    this.activeIndex = index;
    this.restartCarousel();
    this.cdr.markForCheck();
  }

  prev(): void {
    this.activeIndex = (this.activeIndex - 1 + this.banners.length) % this.banners.length;
    this.restartCarousel();
    this.cdr.markForCheck();
  }

  next(): void {
    this.activeIndex = (this.activeIndex + 1) % this.banners.length;
    this.restartCarousel();
    this.cdr.markForCheck();
  }

  pauseCarousel(): void {
    this.stopCarousel();
  }

  resumeCarousel(): void {
    this.startCarousel();
  }

  navigate(banner: OfferBannerDto): void {
    if (banner.linkedProductId) {
      this.router.navigate(['/products', banner.linkedProductId]);
    } else if (banner.linkedCategoryId) {
      this.router.navigate(['/products'], { queryParams: { categoryId: banner.linkedCategoryId } });
    } else if (banner.ctaUrl) {
      if (banner.ctaUrl.startsWith('/')) {
        this.router.navigateByUrl(banner.ctaUrl);
      } else {
        window.open(banner.ctaUrl, '_blank', 'noopener');
      }
    }
  }

  navigateCta(banner: OfferBannerDto): void {
    this.navigate(banner);
  }

  navigateSecondary(banner: OfferBannerDto): void {
    const url = banner.secondaryUrl;
    if (!url) return;
    if (url.startsWith('/')) {
      this.router.navigateByUrl(url);
    } else {
      window.open(url, '_blank', 'noopener');
    }
  }

  private startCarousel(): void {
    if (this.timer !== null || this.banners.length <= 1) return;
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

  onImgError(event: Event, fallback = 'http://localhost:5000/uploads/sample-hero-banner.jpg'): void {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.onerror = null;
      img.src = fallback;
    }
  }
}
