import {
  Component, OnInit, OnDestroy,
  ChangeDetectionStrategy, ChangeDetectorRef,
} from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { forkJoin } from 'rxjs';
import { CategoryService } from '../../core/category/category.service';
import { CurrencyService } from '../../core/currency/currency.service';
import {
  Category,
  PaginationMeta,
  ProductFilter,
  ProductListItem,
  ProductSortBy,
} from '../../shared/models/product.models';
import { ProductService } from '../products/product.service';
import { OfferBannerService, BannersBySlot } from '../../core/banner/banner.service';


@Component({
  selector: 'app-home',
  templateUrl: './home.html',
  styleUrls: ['./home.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Home implements OnInit, OnDestroy {
  categories: Category[] = [];

  bannersBySlot: BannersBySlot | null = null;
  loadingBanners = true;

  trending: ProductListItem[] = [];
  activeHero = 0;
  private heroTimer: number | null = null;
  loadingTrending = true;

  bestSellers: ProductListItem[] = [];
  newlyAdded:  ProductListItem[] = [];
  topRated:    ProductListItem[] = [];
  loadingRows = true;

  private readonly ROW_PAGE_SIZE = 8;
  private bestSellersPage = 1;
  private newlyAddedPage  = 1;
  private topRatedPage    = 1;
  hasMoreBestSellers = false;
  hasMoreNewlyAdded  = false;
  hasMoreTopRated    = false;
  loadingMoreBestSellers = false;
  loadingMoreNewlyAdded  = false;
  loadingMoreTopRated    = false;

  filtered: ProductListItem[] = [];
  filteredPagination: PaginationMeta | null = null;
  loadingFiltered = false;
  hasActiveFilter = false;
  private filter: ProductFilter = { sortBy: 'Newest', page: 1, pageSize: 20 };

  constructor(
    private route:          ActivatedRoute,
    private productService: ProductService,
    private categoryService: CategoryService,
    readonly cs:            CurrencyService,
    private cdr:            ChangeDetectorRef,
    private bannerService:  OfferBannerService,
  ) {}

  ngOnInit(): void {
    this.categoryService.getAll().subscribe({
      next: cats => { this.categories = cats; this.cdr.markForCheck(); },
      error: () => {},
    });

    this.route.queryParams.subscribe(params => {
      this.filter = this.buildFilterFromParams(params);
      this.hasActiveFilter = Object.keys(params).length > 0;
      if (this.hasActiveFilter) this.loadFiltered();
      else { this.filtered = []; this.filteredPagination = null; this.cdr.markForCheck(); }
    });

    this.loadAllRows();
    this.loadOfferBanners();
  }

  // ── Offer banners ────────────────────────────────────────────────
  private loadOfferBanners(): void {
    this.loadingBanners = true;
    this.bannerService.loadPublicBanners().subscribe({
      next: banners => {
        this.bannersBySlot = banners;
        this.loadingBanners = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.loadingBanners = false;
        this.cdr.markForCheck();
      },
    });
  }

  // ── Hero carousel ─────────────────────────────────────────────────────────
  prevHero(): void {
    if (!this.trending.length) return;
    this.activeHero = (this.activeHero - 1 + this.trending.length) % this.trending.length;
    this.restartHeroSlide();
    this.cdr.markForCheck();
  }

  nextHero(): void {
    if (!this.trending.length) return;
    this.activeHero = (this.activeHero + 1) % this.trending.length;
    this.restartHeroSlide();
    this.cdr.markForCheck();
  }

  setHero(i: number): void {
    this.activeHero = i;
    this.restartHeroSlide();
    this.cdr.markForCheck();
  }

  private startHeroSlide(): void {
    if (this.heroTimer !== null || this.trending.length <= 1) return;
    this.heroTimer = window.setInterval(() => {
      this.activeHero = (this.activeHero + 1) % this.trending.length;
      this.cdr.markForCheck();
    }, 3500);
  }

  private restartHeroSlide(): void {
    if (this.heroTimer !== null) { clearInterval(this.heroTimer); this.heroTimer = null; }
    this.startHeroSlide();
  }

  ngOnDestroy(): void {
    if (this.heroTimer !== null) clearInterval(this.heroTimer);
  }

  // ── Category row load-more ────────────────────────────────────────────────
  loadMoreBestSellers(): void {
    this.loadingMoreBestSellers = true;
    this.cdr.markForCheck();
    this.bestSellersPage++;
    this.productService.getAll({ sortBy: 'BestSelling', pageSize: this.ROW_PAGE_SIZE, page: this.bestSellersPage }).subscribe({
      next: res => {
        this.bestSellers = [...this.bestSellers, ...res.data];
        this.hasMoreBestSellers = res.pagination.page < res.pagination.totalPages;
        this.loadingMoreBestSellers = false;
        this.cdr.markForCheck();
      },
      error: () => { this.loadingMoreBestSellers = false; this.cdr.markForCheck(); },
    });
  }

  loadMoreNewlyAdded(): void {
    this.loadingMoreNewlyAdded = true;
    this.cdr.markForCheck();
    this.newlyAddedPage++;
    this.productService.getAll({ sortBy: 'Newest', pageSize: this.ROW_PAGE_SIZE, page: this.newlyAddedPage }).subscribe({
      next: res => {
        this.newlyAdded = [...this.newlyAdded, ...res.data];
        this.hasMoreNewlyAdded = res.pagination.page < res.pagination.totalPages;
        this.loadingMoreNewlyAdded = false;
        this.cdr.markForCheck();
      },
      error: () => { this.loadingMoreNewlyAdded = false; this.cdr.markForCheck(); },
    });
  }

  loadMoreTopRated(): void {
    this.loadingMoreTopRated = true;
    this.cdr.markForCheck();
    this.topRatedPage++;
    this.productService.getAll({ sortBy: 'Rating', pageSize: this.ROW_PAGE_SIZE, page: this.topRatedPage }).subscribe({
      next: res => {
        this.topRated = [...this.topRated, ...res.data];
        this.hasMoreTopRated = res.pagination.page < res.pagination.totalPages;
        this.loadingMoreTopRated = false;
        this.cdr.markForCheck();
      },
      error: () => { this.loadingMoreTopRated = false; this.cdr.markForCheck(); },
    });
  }

  // ── Data loading ──────────────────────────────────────────────────────────
  private loadAllRows(): void {
    this.loadingTrending = true;
    this.loadingRows     = true;

    forkJoin({
      bestSeller: this.productService.getAll({ sortBy: 'BestSelling', pageSize: this.ROW_PAGE_SIZE }),
      newest:     this.productService.getAll({ sortBy: 'Newest',      pageSize: this.ROW_PAGE_SIZE }),
      topRated:   this.productService.getAll({ sortBy: 'Rating',      pageSize: this.ROW_PAGE_SIZE }),
    }).subscribe({
      next: ({ bestSeller, newest, topRated }) => {
        const sliderItems: ProductListItem[] = [];
        if (bestSeller.data[0]) sliderItems.push(bestSeller.data[0]);
        if (newest.data[0])     sliderItems.push(newest.data[0]);
        if (topRated.data[0])   sliderItems.push(topRated.data[0]);
        if (bestSeller.data[1]) sliderItems.push(bestSeller.data[1]);
        if (newest.data[1])     sliderItems.push(newest.data[1]);

        this.trending    = sliderItems;
        this.bestSellers = bestSeller.data;
        this.newlyAdded  = newest.data;
        this.topRated    = topRated.data;

        this.hasMoreBestSellers = bestSeller.pagination.page < bestSeller.pagination.totalPages;
        this.hasMoreNewlyAdded  = newest.pagination.page    < newest.pagination.totalPages;
        this.hasMoreTopRated    = topRated.pagination.page  < topRated.pagination.totalPages;

        this.loadingTrending = false;
        this.loadingRows     = false;
        this.activeHero = 0;
        this.startHeroSlide();
        this.cdr.markForCheck();
      },
      error: () => {
        this.loadingTrending = false;
        this.loadingRows     = false;
        this.cdr.markForCheck();
      },
    });
  }

  private loadFiltered(append = false): void {
    if (!append) this.loadingFiltered = true;
    this.cdr.markForCheck();

    this.productService.getAll(this.filter).subscribe({
      next: res => {
        this.filtered           = append ? [...this.filtered, ...res.data] : res.data;
        this.filteredPagination = res.pagination;
        this.loadingFiltered    = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.loadingFiltered = false;
        this.cdr.markForCheck();
      },
    });
  }

  loadMoreFiltered(): void {
    if (!this.filteredPagination) return;
    this.filter = { ...this.filter, page: (this.filter.page ?? 1) + 1 };
    this.loadFiltered(true);
  }

  get hasMoreFiltered(): boolean {
    return !!this.filteredPagination
      && this.filteredPagination.page < this.filteredPagination.totalPages;
  }

  onCategorySelect(categoryId: number | null): void {
    this.filter = { ...this.filter, categoryId: categoryId ?? undefined, page: 1 };
    this.hasActiveFilter = true;
    this.loadFiltered();
  }

  private buildFilterFromParams(params: Params): ProductFilter {
    const sortRaw = (params['sortBy'] ?? '').toString();
    const allowed: ProductSortBy[] = ['Newest', 'PriceAsc', 'PriceDesc', 'Rating', 'BestSelling'];
    const sortBy = allowed.includes(sortRaw as ProductSortBy)
      ? (sortRaw as ProductSortBy) : 'Newest';
    const categoryId = params['categoryId'] ? +params['categoryId'] : undefined;
    const minRating  = params['minRating']  ? +params['minRating']  : undefined;
    return { sortBy, categoryId, minRating, page: 1, pageSize: 20 };
  }

  trackByProductId = (_: number, p: ProductListItem) => p.productId;
}
