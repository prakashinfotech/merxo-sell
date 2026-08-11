import {
  Component, Input,
  ChangeDetectionStrategy,
} from '@angular/core';
import { Router } from '@angular/router';
import { OfferBannerDto } from '../../../../core/banner/banner.service';

@Component({
  selector: 'app-strip-banner',
  templateUrl: './strip-banner.html',
  styleUrls: ['./strip-banner.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StripBanner {
  @Input() banner!: OfferBannerDto;

  constructor(private router: Router) {}

  navigate(): void {
    if (!this.banner) return;
    if (this.banner.linkedProductId) {
      this.router.navigate(['/products', this.banner.linkedProductId]);
    } else if (this.banner.linkedCategoryId) {
      this.router.navigate(['/products'], {
        queryParams: { categoryId: this.banner.linkedCategoryId },
      });
    } else if (this.banner.ctaUrl) {
      if (this.banner.ctaUrl.startsWith('/')) {
        this.router.navigateByUrl(this.banner.ctaUrl);
      } else {
        window.open(this.banner.ctaUrl, '_blank', 'noopener');
      }
    }
  }
}
