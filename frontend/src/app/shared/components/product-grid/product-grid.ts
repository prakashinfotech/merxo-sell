import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { ProductListItem } from '../../models/product.models';

@Component({
  selector: 'app-product-grid',
  templateUrl: './product-grid.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductGrid {
  @Input() products: ProductListItem[] = [];
  @Input() loading = false;

  readonly skeletons = Array(8);
}
