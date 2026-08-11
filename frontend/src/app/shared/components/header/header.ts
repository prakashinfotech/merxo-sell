import { Component, ChangeDetectionStrategy, signal, OnInit, OnDestroy, HostListener } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { Observable, Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged, filter, map, startWith, switchMap } from 'rxjs/operators';
import { AuthService } from '../../../core/auth/auth.service';
import { CartService } from '../../../core/cart/cart.service';
import { CategoryService } from '../../../core/category/category.service';
import { ProductService } from '../../../features/products/product.service';
import { Category, SuggestItem } from '../../models/product.models';

@Component({
  selector: 'app-header',
  templateUrl: './header.html',
  styleUrls: ['./header.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Header implements OnInit, OnDestroy {
  isAdminRoute  = signal(false);
  isSellerRoute = signal(false);
  isHiddenPage  = signal(false);
  isStorefront  = signal(false);

  showCategoryDropdown = signal(false);
  categories = signal<Category[]>([]);
  activeCategoryId = signal<number | null>(null);
  activeSort = signal<string | null>(null);
  activeRating = signal<number | null>(null);

  // Search state
  searchTerm = '';
  suggestions = signal<SuggestItem[]>([]);
  showSuggestions = signal(false);
  private search$ = new Subject<string>();
  private searchSub?: Subscription;

  // Auth state — re-read on every NavigationEnd so OnPush reliably refreshes
  // the Profile / Login / Register buttons after a successful login redirect.
  loggedIn = signal(false);
  isBuyerSession = signal(false);
  userName = signal('');

  cartCount$: Observable<number>;

  constructor(
    public auth: AuthService,
    public cart: CartService,
    private categoryService: CategoryService,
    private productService: ProductService,
    private router: Router,
    private route: ActivatedRoute,
  ) {
    this.cartCount$ = this.cart.itemCount$.pipe(startWith(0));
  }

  ngOnDestroy(): void {
    this.searchSub?.unsubscribe();
  }

  ngOnInit(): void {
    this.updateState(this.router.url);

    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd), map(e => (e as NavigationEnd).urlAfterRedirects))
      .subscribe(url => this.updateState(url));

    this.route.queryParams.subscribe(params => {
      this.activeCategoryId.set(params['categoryId'] ? +params['categoryId'] : null);
      this.activeSort.set(params['sortBy'] || null);
      this.activeRating.set(params['minRating'] ? +params['minRating'] : null);
    });

    if (this.auth.isLoggedIn() && this.auth.isBuyer()) {
      this.cart.fetchCart().subscribe({ error: () => {} });
    }

    this.categoryService.getAll().subscribe({
      next: cats => this.categories.set(cats),
      error: () => {},
    });

    // Debounced 300ms autosuggest stream — empty input clears results.
    this.searchSub = this.search$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(q => {
        const trimmed = (q ?? '').trim();
        if (trimmed.length < 2) {
          this.suggestions.set([]);
          this.showSuggestions.set(false);
          return [];
        }
        return this.productService.suggest(trimmed);
      })
    ).subscribe({
      next: items => {
        this.suggestions.set(items as SuggestItem[]);
        this.showSuggestions.set((items as SuggestItem[]).length > 0);
      },
      error: () => {
        this.suggestions.set([]);
        this.showSuggestions.set(false);
      }
    });
  }

  onSearchInput(value: string): void {
    this.searchTerm = value;
    this.search$.next(value);
  }

  onSearchSubmit(): void {
    const q = this.searchTerm.trim();
    this.showSuggestions.set(false);
    if (!q) {
      this.router.navigate(['/products']);
    } else {
      this.router.navigate(['/products'], { queryParams: { q } });
    }
  }

  pickSuggestion(item: SuggestItem): void {
    this.showSuggestions.set(false);
    if (item.type === 'product') {
      this.router.navigate(['/products', item.id]);
    } else {
      this.router.navigate(['/products'], { queryParams: { categoryId: item.id } });
    }
  }

  hideSuggestions(): void {
    // Tiny delay so click on a suggestion still fires.
    setTimeout(() => this.showSuggestions.set(false), 150);
  }

  /** Apply a sort filter and route the user to the home page so the result is visible. */
  applySort(sortBy: 'Newest' | 'BestSelling' | 'Rating'): void {
    this.router.navigate(['/'], { queryParams: { sortBy } });
  }

  /** Apply a 5-star filter via query param. */
  applyFiveStar(): void {
    this.router.navigate(['/'], { queryParams: { minRating: 5 } });
  }

  /** Apply a category filter. */
  applyCategory(categoryId: number | null): void {
    this.showCategoryDropdown.set(false);
    if (categoryId == null) this.router.navigate(['/']);
    else this.router.navigate(['/'], { queryParams: { categoryId } });
  }

  toggleCategoryDropdown(event: Event): void {
    event.stopPropagation();
    this.showCategoryDropdown.update(v => !v);
  }

  @HostListener('document:click')
  closeDropdowns(): void {
    if (this.showCategoryDropdown()) {
      this.showCategoryDropdown.set(false);
    }
  }

  logout(): void {
    this.cart.resetLocal();
    this.auth.logout();
  }

  goCart(): void {
    if (!this.auth.isLoggedIn() || !this.auth.isBuyer()) {
      this.router.navigate(['/auth/login'], { queryParams: { returnUrl: '/cart' } });
      return;
    }
    this.router.navigate(['/cart']);
  }

  private updateState(url: string): void {
    const isAdmin   = url.startsWith('/admin');
    const isSeller  = url.startsWith('/seller');
    const isAuth    = url.startsWith('/auth');

    this.isAdminRoute.set(isAdmin);
    this.isSellerRoute.set(isSeller);
    this.isHiddenPage.set(isAuth || isAdmin || isSeller);
    this.isStorefront.set(!isAdmin && !isSeller && !isAuth);

    // Re-evaluate auth on every nav so OnPush picks up the post-login state.
    this.loggedIn.set(this.auth.isLoggedIn());
    this.isBuyerSession.set(this.auth.isBuyer());
    const email = this.auth.getEmail();
    const local = email ? email.split('@')[0] : '';
    this.userName.set(local.replace(/[._-]/g, ' ').replace(/\b\w/g, c => c.toUpperCase()));

    this.showCategoryDropdown.set(false);
  }
}
