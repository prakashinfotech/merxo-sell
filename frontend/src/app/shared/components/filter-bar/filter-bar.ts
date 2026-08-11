import { Component, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { ProductFilter, ProductSortBy } from '../../models/product.models';

@Component({
  selector: 'app-filter-bar',
  templateUrl: './filter-bar.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FilterBar {
  @Output() filterChange = new EventEmitter<ProductFilter>();

  sortBy: ProductSortBy = 'Newest';
  search = '';

  readonly sortOptions: { label: string; value: ProductSortBy }[] = [
    { label: 'Newest', value: 'Newest' },
    { label: 'Price: Low', value: 'PriceAsc' },
    { label: 'Price: High', value: 'PriceDesc' },
    { label: 'Top Rated', value: 'Rating' },
    { label: 'Best Selling', value: 'BestSelling' }
  ];

  onSortChange(value: string): void {
    this.sortBy = value as ProductSortBy;
    this.emit();
  }

  onSearch(value: string): void {
    this.search = value;
    this.emit();
  }

  private emit(): void {
    this.filterChange.emit({ sortBy: this.sortBy, search: this.search || undefined });
  }
}
