import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { AdminCategories } from './admin-categories';
import { AdminCategoriesService, AdminCategoryDto } from '../../../core/admin/admin-categories.service';
import { DialogService } from '../../../core/dialog/dialog.service';

const mockCategories: AdminCategoryDto[] = [
  { categoryId: 1, name: 'Electronics', slug: 'electronics', parentCategoryId: undefined,
    sortOrder: 1, isActive: true, productCount: 5, subCategoryCount: 1 },
  { categoryId: 2, name: 'Phones', slug: 'phones', parentCategoryId: 1,
    sortOrder: 1, isActive: true, productCount: 3, subCategoryCount: 0 },
  { categoryId: 3, name: 'Clothing', slug: 'clothing', parentCategoryId: undefined,
    sortOrder: 2, isActive: true, productCount: 12, subCategoryCount: 0 },
];

describe('AdminCategories component', () => {
  let component: AdminCategories;
  let fixture: ComponentFixture<AdminCategories>;
  let service: any;
  let dialog: any;

  beforeEach(async () => {
    service = {
      getAll:  vi.fn(() => of(mockCategories)),
      create:  vi.fn(() => of(mockCategories[0])),
      update:  vi.fn(() => of(mockCategories[0])),
      delete:  vi.fn(() => of(void 0)),
    };

    dialog = {
      ask: vi.fn(() => of(true)),
    };

    await TestBed.configureTestingModule({
      declarations: [AdminCategories],
      providers: [
        { provide: AdminCategoriesService, useValue: service },
        { provide: DialogService,          useValue: dialog },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture   = TestBed.createComponent(AdminCategories);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('ngOnInit() loads categories', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    expect(component.categories).toHaveLength(3);
  });

  it('filteredCategories returns all when no search term', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    component.searchTerm = '';
    expect(component.filteredCategories).toHaveLength(3);
  });

  it('filteredCategories filters by name', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    component.searchTerm = 'phone';
    expect(component.filteredCategories).toHaveLength(1);
    expect(component.filteredCategories[0].name).toBe('Phones');
  });

  it('scopeFilter roots returns only root categories', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    component.scopeFilter = 'roots';
    const roots = component.filteredCategories;
    expect(roots.every(c => c.parentCategoryId == null)).toBe(true);
  });

  it('scopeFilter subs returns only subcategories', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    component.scopeFilter = 'subs';
    const subs = component.filteredCategories;
    expect(subs.every(c => c.parentCategoryId != null)).toBe(true);
  });
});
