import { Component, OnInit, signal } from '@angular/core';
import { CurrencyService } from './core/currency/currency.service';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.scss'
})
export class App implements OnInit {
  isPortal = signal(false);
  isAuth = signal(false);

  constructor(
    private currencyService: CurrencyService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currencyService.initialize();
    this.updatePortalState(this.router.url);

    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.updatePortalState(event.urlAfterRedirects);
    });
  }

  private updatePortalState(url: string): void {
    this.isPortal.set(url.startsWith('/admin') || url.startsWith('/seller'));
    this.isAuth.set(url.startsWith('/auth'));
  }
}
