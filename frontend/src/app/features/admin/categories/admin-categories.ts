import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { AdminCategoriesService, AdminCategoryDto, CreateCategoryDto, UpdateCategoryDto } from '../../../core/admin/admin-categories.service';
import { DialogService } from '../../../core/dialog/dialog.service';
import { ToastService } from '../../../core/toast/toast.service';

interface CategoryForm {
  name: string;
  slug: string;
  imageUrl: string;
  parentCategoryId: number | null;
  sortOrder: number;
  isActive: boolean;
}

@Component({
  selector: 'app-admin-categories',
  standalone: false,
  templateUrl: './admin-categories.html',
  styleUrl: './admin-categories.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminCategories implements OnInit {
  categories: AdminCategoryDto[] = [];
  loading = true;
  error = '';

  searchTerm = '';
  scopeFilter: 'all' | 'roots' | 'subs' = 'all';
  readonly pageSize = 8;
  page = 1;

  showForm = false;
  editing: AdminCategoryDto | null = null;
  viewing: AdminCategoryDto | null = null;
  saving = false;

  form: CategoryForm = this.emptyForm();
  expandedRows = new Set<number>();

  constructor(
    private service: AdminCategoriesService,
    private dialog: DialogService,
    private cdr: ChangeDetectorRef,
    private toast: ToastService,
  ) {}

  ngOnInit(): void {
    this.load();
  }

  get filteredCategories(): AdminCategoryDto[] {
    let list = this.categories;

    if (this.scopeFilter === 'roots') list = list.filter(c => c.parentCategoryId == null);
    else if (this.scopeFilter === 'subs') list = list.filter(c => c.parentCategoryId != null);

    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return list;
    return list.filter(c =>
      c.name.toLowerCase().includes(term) ||
      c.slug.toLowerCase().includes(term) ||
      (c.parentName ?? '').toLowerCase().includes(term));
  }

  get rootCategories(): AdminCategoryDto[] {
    return this.categories.filter(c => c.parentCategoryId == null);
  }

  get displayedCategories(): AdminCategoryDto[] {
    const list = this.filteredCategories;
    
    // If we have a search term or specific scope filter (subs), show flat list
    if (this.searchTerm.trim() || this.scopeFilter !== 'all') {
      return list;
    }

    // Hierarchical display: Roots first, then children if expanded
    const result: AdminCategoryDto[] = [];
    const roots = list.filter(c => c.parentCategoryId == null);
    
    roots.forEach(root => {
      result.push(root);
      if (this.isExpanded(root.categoryId)) {
        const subs = this.categories.filter(c => c.parentCategoryId === root.categoryId);
        result.push(...subs);
      }
    });

    return result;
  }

  get totalPages(): number { return Math.max(1, Math.ceil(this.displayedCategories.length / this.pageSize)); }
  get pagedCategories(): AdminCategoryDto[] {
    const start = (this.page - 1) * this.pageSize;
    return this.displayedCategories.slice(start, start + this.pageSize);
  }
  goPage(page: number): void {
    this.page = Math.min(Math.max(page, 1), this.totalPages);
    this.cdr.markForCheck();
  }
  onFilterChanged(): void {
    this.page = 1;
    this.cdr.markForCheck();
  }

  toggleRow(id: number): void {
    if (this.expandedRows.has(id)) this.expandedRows.delete(id);
    else this.expandedRows.add(id);
    this.cdr.markForCheck();
  }

  isExpanded(id: number): boolean {
    return this.expandedRows.has(id);
  }

  load(): void {
    this.loading = true;
    this.service.getAll().subscribe({
      next: list => {
        this.categories = list;
        this.page = 1;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.toast.error('Failed to load categories.');
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  onScopeChange(scope: 'all' | 'roots' | 'subs'): void {
    this.scopeFilter = scope;
    this.onFilterChanged();
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.scopeFilter = 'all';
    this.onFilterChanged();
  }

  openCreate(): void {
    this.editing = null;
    this.form = this.emptyForm();
    this.showForm = true;
    this.error = '';
    this.cdr.markForCheck();
  }

  openCreateSubFor(parent: AdminCategoryDto): void {
    this.editing = null;
    this.form = { ...this.emptyForm(), parentCategoryId: parent.categoryId };
    this.showForm = true;
    this.error = '';
    this.cdr.markForCheck();
  }

  openEdit(c: AdminCategoryDto): void {
    this.editing = c;
    this.form = {
      name: c.name,
      slug: c.slug,
      imageUrl: c.imageUrl ?? '',
      parentCategoryId: c.parentCategoryId ?? null,
      sortOrder: c.sortOrder,
      isActive: c.isActive,
    };
    this.showForm = true;
    this.error = '';
    this.cdr.markForCheck();
  }

  openView(c: AdminCategoryDto): void {
    this.viewing = c;
    this.cdr.markForCheck();
  }

  closeView(): void {
    this.viewing = null;
    this.cdr.markForCheck();
  }

  cancelForm(): void {
    this.showForm = false;
    this.editing = null;
    this.cdr.markForCheck();
  }

  save(): void {
    if (!this.form.name.trim()) {
      this.error = 'Category name is required.';
      this.cdr.markForCheck();
      return;
    }

    this.saving = true;
    const dto: CreateCategoryDto = {
      name: this.form.name.trim(),
      slug: this.form.slug.trim() || undefined,
      imageUrl: this.form.imageUrl.trim() || undefined,
      parentCategoryId: this.form.parentCategoryId ?? null,
      sortOrder: this.form.sortOrder ?? 0,
      isActive: this.form.isActive,
    };

    const obs = this.editing
      ? this.service.update(this.editing.categoryId, dto as UpdateCategoryDto)
      : this.service.create(dto);

    obs.subscribe({
      next: () => {
        this.toast.success(this.editing ? `${dto.name} updated.` : `${dto.name} added.`);
        this.saving = false;
        this.showForm = false;
        this.editing = null;
        this.load();
      },
      error: err => {
        this.error = err?.error?.error || 'Save failed. Please try again.';
        this.saving = false;
        this.cdr.markForCheck();
      },
    });
  }

  toggleActive(c: AdminCategoryDto): void {
    const dto: UpdateCategoryDto = {
      name: c.name,
      slug: c.slug,
      imageUrl: c.imageUrl,
      parentCategoryId: c.parentCategoryId ?? null,
      sortOrder: c.sortOrder,
      isActive: !c.isActive,
    };
    this.service.update(c.categoryId, dto).subscribe({
      next: () => {
        this.toast.success(`${c.name} ${dto.isActive ? 'activated' : 'deactivated'}.`);
        this.load();
      },
      error: () => {
        this.toast.error('Status change failed.');
        this.cdr.markForCheck();
      },
    });
  }

  delete(c: AdminCategoryDto): void {
    this.dialog.ask(`Deactivate Category`, `Deactivate "${c.name}"? Products and sub-categories must be removed first.`, true)
      .subscribe(confirmed => {
        if (!confirmed) return;
        
        this.service.delete(c.categoryId).subscribe({
          next: () => {
            this.toast.success(`${c.name} deactivated.`);
            this.load();
          },
          error: err => {
            this.toast.error(err?.error?.error || 'Delete operation failed.');
            this.cdr.markForCheck();
          },
        });
      });
  }

  trackById = (_: number, c: AdminCategoryDto) => c.categoryId;

  private emptyForm(): CategoryForm {
    return {
      name: '',
      slug: '',
      imageUrl: '',
      parentCategoryId: null,
      sortOrder: 0,
      isActive: true,
    };
  }
}
