import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login.component.html',
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  username = '';
  password = '';
  errorMessage = '';
  isSubmitting = false;

  async login(): Promise<void> {
    this.errorMessage = '';

    const username = this.username.trim();
    const password = this.password;

    if (!username || !password) {
      this.errorMessage = 'Username and password are required.';
      return;
    }

    this.isSubmitting = true;

    try {
      await firstValueFrom(this.authService.login(username, password));
      await this.router.navigate(['/']);
    } catch {
      this.errorMessage = 'Login failed.';
    } finally {
      this.isSubmitting = false;
    }
  }
}
