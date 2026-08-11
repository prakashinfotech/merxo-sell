import { Component, Input, OnChanges, SimpleChanges, ChangeDetectionStrategy } from '@angular/core';
import { ProductImage } from '../../../shared/models/product.models';

@Component({
  selector: 'app-image-gallery',
  templateUrl: './image-gallery.html',
  styleUrl: './image-gallery.scss',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ImageGallery implements OnChanges {
  @Input() images: ProductImage[] = [];
  activeIndex = 0;

  ngOnChanges(changes: SimpleChanges): void {
    // Reset to first image whenever the gallery is swapped (e.g. variant change).
    if (changes['images']) this.activeIndex = 0;
  }

  select(i: number): void { this.activeIndex = i; }
}
