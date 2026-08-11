import {
  Component, OnInit, ElementRef, ViewChild,
  ChangeDetectorRef, ChangeDetectionStrategy, HostListener,
} from '@angular/core';
import {
  SellerService,
  SellerDashboardDto,
  SellerOrderListDto,
  SellerAnalyticsSummaryDto,
  SellerDashboardFilter,
  SellerDashboardRange,
} from '../../../core/seller/seller.service';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

interface RangeOption { key: SellerDashboardRange; label: string; }

@Component({
  selector: 'app-seller-dashboard',
  standalone: false,
  templateUrl: './seller-dashboard.html',
  styleUrls: ['./seller-dashboard.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SellerDashboard implements OnInit {
  @ViewChild('revenueBarChart') revenueBarChartCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('donutChart')      donutChartCanvas!:      ElementRef<HTMLCanvasElement>;

  stats:        SellerDashboardDto        | null = null;
  analytics:    SellerAnalyticsSummaryDto | null = null;
  recentOrders: SellerOrderListDto[]             = [];

  loading = true;
  error   = false;

  readonly rangeOptions: RangeOption[] = [
    { key: '7d',         label: 'Last 7 days'  },
    { key: '30d',        label: 'Last 30 days' },
    { key: 'this-month', label: 'This Month'   },
    { key: 'month',      label: 'Last Month'   },
    { key: 'custom',     label: 'Custom Range' },
  ];
  filter: SellerDashboardFilter = { range: '30d' };
  showRangeMenu = false;
  customFrom: Date | null = null;
  customTo:   Date | null = null;

  private charts: Chart[] = [];

  readonly catColors = ['#0B0B0B', '#C8C2B0', '#E8E2D6', '#C7F542', '#F4F1E8', '#767676'];

  constructor(
    private service: SellerService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void { this.loadAll(); }

  // ── Filter helpers ──────────────────────────────────────────────
  get rangeLabel(): string {
    return this.rangeOptions.find(r => r.key === this.filter.range)?.label ?? 'Last 30 days';
  }

  get totalRevenueSummary(): string {
    return this.analytics ? this.formatCompact(this.analytics.totalSalesCAD) : '—';
  }

  toggleRangeMenu(event: Event): void {
    event.stopPropagation();
    this.showRangeMenu = !this.showRangeMenu;
    this.cdr.markForCheck();
  }

  pickRange(key: SellerDashboardRange): void {
    if (key === 'custom') {
      this.filter = { ...this.filter, range: 'custom' };
      this.cdr.markForCheck();
      return;
    }
    this.filter = { range: key };
    this.showRangeMenu = false;
    this.loadAll();
  }

  applyCustom(): void {
    if (!this.customFrom || !this.customTo || this.customFrom > this.customTo) return;
    const fmt = (d: Date) => d.toISOString().slice(0, 10);
    this.filter = { range: 'custom', from: fmt(this.customFrom), to: fmt(this.customTo) };
    this.showRangeMenu = false;
    this.loadAll();
  }

  @HostListener('document:click')
  closeRangeMenu(): void {
    if (this.showRangeMenu) { this.showRangeMenu = false; this.cdr.markForCheck(); }
  }

  // ── Formatting ──────────────────────────────────────────────────
  formatCompact(value: number): string {
    if (value >= 1_000_000) return `$${(value / 1_000_000).toFixed(1)}M`;
    if (value >= 1_000)     return `$${(value / 1_000).toFixed(1)}K`;
    return `$${Math.round(value)}`;
  }

  formatDate(iso: string): string {
    const d = new Date(iso);
    if (Number.isNaN(d.getTime())) return iso;
    return d.toLocaleDateString(undefined, { day: '2-digit', month: 'short', year: '2-digit' });
  }

  getCatColor(index: number): string {
    return this.catColors[index % this.catColors.length];
  }

  getStatusClass(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'delivered':  return 'done';
      case 'shipped':
      case 'processing': return 'prog';
      case 'pending':    return 'wait';
      default:           return 'fail';
    }
  }

  // ── Data loading ────────────────────────────────────────────────
  loadAll(): void {
    this.loading = true;
    this.error   = false;
    this.cdr.markForCheck();
    this.destroyCharts();

    let pending = 3;
    const finish = () => {
      if (--pending === 0) {
        this.loading = false;
        this.cdr.detectChanges();
        setTimeout(() => { this.renderRevenueChart(); this.renderDonutChart(); }, 0);
      }
    };

    this.service.getDashboard().subscribe({
      next: s  => { this.stats = s; finish(); },
      error: () => { this.error = true; finish(); },
    });

    this.service.getAnalyticsSummary(this.filter).subscribe({
      next: a  => { this.analytics = a; finish(); },
      error: () => { this.analytics = null; finish(); },
    });

    this.service.getOrders().subscribe({
      next: o  => { this.recentOrders = o; finish(); },
      error: () => { this.recentOrders = []; finish(); },
    });
  }

  // ── Charts ──────────────────────────────────────────────────────
  private renderRevenueChart(): void {
    if (!this.revenueBarChartCanvas || !this.analytics) return;
    const ctx = this.revenueBarChartCanvas.nativeElement.getContext('2d');
    if (!ctx) return;

    const series = this.analytics.revenueByDay;
    const labels = series.map(d =>
      new Date(d.date).toLocaleDateString(undefined, { month: 'short', day: 'numeric' }));

    this.charts[0] = new Chart(ctx, {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: 'Revenue',
            data: series.map(d => d.revenueCAD),
            backgroundColor: '#0B0B0B',
            borderRadius: 6,
            borderSkipped: false,
            barPercentage: 0.60,
            categoryPercentage: 0.80,
          },
          {
            label: 'Orders (scaled)',
            data: series.map(d => d.orderCount * 15),
            backgroundColor: 'rgba(0,0,0,0.13)',
            borderRadius: 4,
            borderSkipped: false,
            barPercentage: 0.60,
            categoryPercentage: 0.80,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            backgroundColor: '#0B0B0B',
            padding: 10,
            cornerRadius: 8,
            displayColors: false,
            callbacks: {
              label: (c) => c.datasetIndex === 0
                ? `Revenue: CA$${(c.parsed.y || 0).toLocaleString(undefined, { maximumFractionDigits: 0 })}`
                : `Orders: ${series[c.dataIndex]?.orderCount ?? 0}`,
            },
          },
        },
        scales: {
          x: {
            border: { display: false },
            grid:   { display: false },
            ticks:  { font: { size: 9, weight: 600, family: 'Poppins' }, color: '#94a3b8', maxTicksLimit: 10, padding: 4 },
          },
          y: {
            border: { display: false },
            grid:   { color: 'rgba(229,225,214,0.70)', tickLength: 0 },
            ticks: {
              font: { size: 10, weight: 600, family: 'Poppins' },
              color: '#94a3b8', maxTicksLimit: 5, padding: 8,
              callback: (v) => this.formatCompact(Number(v)),
            },
          },
        },
      },
    });
  }

  private renderDonutChart(): void {
    if (!this.donutChartCanvas || !this.analytics) return;
    const ctx = this.donutChartCanvas.nativeElement.getContext('2d');
    if (!ctx) return;

    const top = this.analytics.topProducts;
    const data = top.length > 0
      ? top
      : [{ productId: 0, name: 'No Data', primaryImageUrl: null, unitsSold: 0, revenueCAD: 1 }];

    this.charts[1] = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: data.map(d => d.name),
        datasets: [{
          data: data.map(d => d.revenueCAD),
          backgroundColor: this.catColors.slice(0, data.length),
          borderWidth: 2,
          borderColor: '#FFF',
          hoverOffset: 8,
        }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '68%',
        plugins: {
          legend: { display: false },
          tooltip: {
            backgroundColor: '#0B0B0B',
            padding: 10,
            cornerRadius: 8,
            callbacks: {
              label: (c) => ` ${c.label}: CA$${Number(c.raw).toLocaleString(undefined, { maximumFractionDigits: 0 })}`,
            },
          },
        },
      },
    });
  }

  private destroyCharts(): void {
    this.charts.forEach(c => c?.destroy());
    this.charts = [];
  }

  @HostListener('window:resize')
  onResize(): void { this.charts.forEach(c => c?.resize()); }

  onImgError(event: Event): void {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.onerror = null;
      img.src = '/assets/images/white-tshirt.png';
    }
  }
}
