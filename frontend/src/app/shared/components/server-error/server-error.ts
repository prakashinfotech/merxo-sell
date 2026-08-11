import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-server-error',
  templateUrl: './server-error.html',
  styleUrls: ['./server-error.scss'],
  standalone: false,
})
export class ServerError {
  constructor(private router: Router) {}

  goHome(): void {
    this.router.navigate(['/']);
  }

  retry(): void {
    window.history.back();
  }
}
