import {
  Component, EventEmitter, Input, OnInit, Output, ChangeDetectionStrategy, ChangeDetectorRef
} from '@angular/core';
import { CategoryService } from '../../../core/category/category.service';
import { Category, ProductSearchFilter, ProductSortBy } from '../../models/product.models';

@Component({
  selector: 'app-filter-sidebar',
  templateUrl: './filter-sidebar.html',
  styleUrls: ['./filter-sidebar.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FilterSidebar implements OnInit {
  @Input() initialFilter: ProductSearchFilter = {};
  @Output() filterChange = new EventEmitter<ProductSearchFilter>();

  categories: Category[] = [];

  minPrice = 0;
  maxPrice = 2000;
  selectedCategoryId: number | null = null;
  inStockOnly = false;
  sortBy: ProductSortBy = 'Newest';
  minRating: number | null = null;

  readonly sortOptions: { label: string; value: ProductSortBy }[] = [
    { label: 'Newest',          value: 'Newest' },
    { label: 'Price: Low → High', value: 'PriceAsc' },
    { label: 'Price: High → Low', value: 'PriceDesc' },
    { label: 'Top Rated',       value: 'Rating' },
    { label: 'Best Selling',    value: 'BestSelling' },
  ];

  constructor(
    private categoryService: CategoryService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    if (this.initialFilter.minPrice != null) this.minPrice = this.initialFilter.minPrice;
    if (this.initialFilter.maxPrice != null) this.maxPrice = this.initialFilter.maxPrice;
    if (this.initialFilter.categoryId != null) this.selectedCategoryId = this.initialFilter.categoryId;
    if (this.initialFilter.inStock != null) this.inStockOnly = this.initialFilter.inStock;
    if (this.initialFilter.sortBy) this.sortBy = this.initialFilter.sortBy;

    this.categoryService.getAll().subscribe({
      next: cats => { this.categories = cats; this.cdr.markForCheck(); },
      error: () => {},
    });
  }

  toggleCategory(id: number): void {
    this.selectedCategoryId = this.selectedCategoryId === id ? null : id;
    this.emit();
  }

  onMinPrice(value: string): void {
    const num = +value;
    this.minPrice = isNaN(num) ? 0 : Math.max(0, num);
    if (this.minPrice > this.maxPrice) this.maxPrice = this.minPrice;
    this.emit();
  }

  onMaxPrice(value: string): void {
    const num = +value;
    this.maxPrice = isNaN(num) ? 0 : Math.max(0, num);
    if (this.maxPrice < this.minPrice) this.minPrice = 0;
    this.emit();
  }

  setRating(star: number): void {
    this.minRating = this.minRating === star ? null : star;
    this.emit();
  }

  setInStock(val: boolean): void {
    this.inStockOnly = val;
    this.emit();
  }

  onSortChange(value: string): void {
    this.sortBy = value as ProductSortBy;
    this.emit();
  }

  resetPrice(): void {
    this.minPrice = 0;
    this.maxPrice = 2000;
    this.emit();
  }

  reset(): void {
    this.minPrice = 0;
    this.maxPrice = 2000;
    this.selectedCategoryId = null;
    this.inStockOnly = false;
    this.sortBy = 'Newest';
    this.minRating = null;
    this.emit();
  }

  private emit(): void {
    this.filterChange.emit({
      minPrice: this.minPrice > 0 ? this.minPrice : undefined,
      maxPrice: this.maxPrice < 5000 ? this.maxPrice : undefined,
      categoryId: this.selectedCategoryId ?? undefined,
      inStock: this.inStockOnly || undefined,
      sortBy: this.sortBy,
    });
  }
}
