import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { SellerProductForm } from './seller-product-form';
import { SellerService } from '../../../core/seller/seller.service';
import { CategoryService } from '../../../core/category/category.service';
import { MediaService } from '../../../core/media/media.service';
import { DialogService } from '../../../core/dialog/dialog.service';

describe('SellerProductForm', () => {
  let component: SellerProductForm;
  let fixture: ComponentFixture<SellerProductForm>;
  let sellerService: any;
  let categoryService: any;

  beforeEach(async () => {
    sellerService = {
      getProducts:    vi.fn(() => of({ data: [], pagination: { page: 1, pageSize: 20, totalCount: 0, totalPages: 0 } })),
      createProduct:  vi.fn(() => of({ productId: 1 })),
      updateProduct:  vi.fn(() => of({ productId: 1 })),
      getProduct:     vi.fn(() => of(null)),
      upsertVariants: vi.fn(() => of([])),
    };

    categoryService = {
      getTree: vi.fn(() => of([
        { categoryId: 1, name: 'Electronics', slug: 'electronics', children: [] }
      ])),
    };

    await TestBed.configureTestingModule({
      declarations: [SellerProductForm],
      imports: [ReactiveFormsModule],
      providers: [
        { provide: SellerService,    useValue: sellerService },
        { provide: CategoryService,  useValue: categoryService },
        { provide: MediaService,     useValue: { uploadFile: vi.fn(() => of({ url: 'https://cdn.example.com/img.png' })) } },
        { provide: DialogService,    useValue: { notify: vi.fn(), ask: vi.fn(() => of(true)) } },
        { provide: ActivatedRoute,   useValue: { snapshot: { paramMap: { get: () => null } }, paramMap: of({ get: () => null }) } },
        { provide: Router,           useValue: { navigate: vi.fn() } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture   = TestBed.createComponent(SellerProductForm);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('form is invalid when name is empty', () => {
    fixture.detectChanges();
    component.form.patchValue({ name: '', basePrice: 10, stock: 5, primaryImageUrl: 'http://img.png', rootCategoryId: 1 });
    expect(component.form.get('name')!.valid).toBe(false);
  });

  it('form is invalid when basePrice is 0', () => {
    fixture.detectChanges();
    component.form.patchValue({ name: 'Test', basePrice: 0, stock: 5, primaryImageUrl: 'http://img.png', rootCategoryId: 1 });
    expect(component.form.get('basePrice')!.valid).toBe(false);
  });

  it('form is valid when all required fields are filled', () => {
    fixture.detectChanges();
    component.form.patchValue({
      name: 'Laptop', basePrice: 999, stock: 10,
      primaryImageUrl: 'http://img.png', rootCategoryId: 1
    });
    expect(component.form.get('name')!.valid).toBe(true);
    expect(component.form.get('basePrice')!.valid).toBe(true);
  });

  it('isEdit is false for a new product (no route param)', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    expect(component.isEdit).toBe(false);
  });

  it('suggestedColors list is non-empty', () => {
    expect(component.suggestedColors.length).toBeGreaterThan(0);
    expect(component.suggestedColors).toContain('Red');
  });

  it('suggestedSizes list includes standard sizes', () => {
    expect(component.suggestedSizes).toContain('M');
    expect(component.suggestedSizes).toContain('XL');
  });
});
