import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  PaginationMeta, ProductListItem, ProductSearchFilter, Category
} from '../../shared/models/product.models';
import { ProductService } from './product.service';
import { CategoryService } from '../../core/category/category.service';

@Component({
  selector: 'app-products',
  templateUrl: './products.html',
  styleUrls: ['./products.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Products implements OnInit {
  products:   ProductListItem[] = [];
  pagination: PaginationMeta | null = null;
  categories: Category[] = [];
  loading = true;
  viewMode: 'grid' | 'list' = 'grid';

  filter: ProductSearchFilter = { sortBy: 'Newest', page: 1, pageSize: 20 };

  readonly skeletons = Array(9);

  constructor(
    private productService:  ProductService,
    private categoryService: CategoryService,
    private route:           ActivatedRoute,
    private router:          Router,
    private cdr:             ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.categoryService.getAll().subscribe({
      next: cats => { this.categories = cats; this.cdr.markForCheck(); },
      error: () => {},
    });

    this.route.queryParams.subscribe(params => {
      this.filter = {
        ...this.filter,
        q:          params['q'] || undefined,
        categoryId: params['categoryId'] ? +params['categoryId'] : undefined,
        page:       1,
      };
      this.loadProducts();
    });
  }

  get activeCategory(): Category | undefined {
    return this.categories.find(c => c.categoryId === this.filter.categoryId);
  }

  selectCategory(id: number): void {
    this.filter = { ...this.filter, categoryId: id, page: 1 };
    this.loadProducts();
  }

  clearCategory(): void {
    this.filter = { ...this.filter, categoryId: undefined, page: 1 };
    this.loadProducts();
  }

  onSidebarFilter(partial: ProductSearchFilter): void {
    this.filter = { ...this.filter, ...partial, page: 1 };
    this.loadProducts();
  }

  onSort(value: string): void {
    this.filter = { ...this.filter, sortBy: value as any, page: 1 };
    this.loadProducts();
  }

  loadMore(): void {
    if (!this.pagination) return;
    this.filter = { ...this.filter, page: (this.filter.page ?? 1) + 1 };
    this.loadProducts(true);
  }

  clearFilters(): void {
    this.filter = { sortBy: 'Newest', page: 1, pageSize: 20 };
    this.loadProducts();
  }

  get hasMore(): boolean {
    return !!this.pagination && this.pagination.page < this.pagination.totalPages;
  }

  get resultLabel(): string {
    if (!this.pagination) return '';
    const t = this.pagination.totalCount;
    return t === 1 ? '1 result' : `${t.toLocaleString()} results`;
  }

  private loadProducts(append = false): void {
    if (!append) this.loading = true;
    this.cdr.markForCheck();

    this.productService.search(this.filter).subscribe({
      next: res => {
        this.products   = append ? [...this.products, ...res.data] : res.data;
        this.pagination = res.pagination;
        this.loading    = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }
}
