import { CommonModule } from '@angular/common';
import { Component, HostBinding, HostListener } from '@angular/core';
import {
  NavigationEnd,
  Router,
  RouterModule,
  RouterOutlet,
} from '@angular/router';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { filter } from 'rxjs';
import { ScreenService } from './core/services/screen.service';
import { SiderMenuComponent } from './shared/components/sider-menu/sider-menu.component';
import { HeaderComponent } from './shared/components/header/header.component';
import { ToastContainerComponent } from "./shared/components/toast-container/toast-container.component";
import { DrawerModule } from 'primeng/drawer';
import { ButtonModule } from 'primeng/button';
import { AvatarModule } from 'primeng/avatar';
import { ToolbarModule } from 'primeng/toolbar';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterModule,
    NzLayoutModule,
    NzMenuModule,
    NzIconModule,
    SiderMenuComponent,
    HeaderComponent,
    ToastContainerComponent,
    DrawerModule, ButtonModule, AvatarModule, ToolbarModule, BreadcrumbModule
],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {
  @HostBinding('class') get getClass() {
    return Object.keys(this.screen.sizes)
      .filter((cl) => this.screen.sizes[cl])
      .join(' ');
  }

  items: MenuItem[] | undefined;

  home: MenuItem | undefined;
  isMobile = false;
  isVisibleMenu = true;
  isRouteReady = false;
  isPublicRoute = false;

  constructor(
    private screen: ScreenService,
    private router: Router,
  ) {
    this.checkIfMobile();

    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        const publicRoutes = [
          '/auth/login',
          '/auth/recovery-password',
          '/auth',
        ];

        this.isPublicRoute = publicRoutes.some((route) =>
          event.urlAfterRedirects.startsWith(route),
        );

        if (this.isMobile) {
          this.isVisibleMenu = false;
        }

        this.isRouteReady = true;
      });
  }

   ngOnInit() {
        this.items = [
            { label: 'Electronics' },
            { label: 'Computer' },
            { label: 'Accessories' },
            { label: 'Keyboard' },
            { label: 'Wireless' }
        ];

        this.home = { icon: 'pi pi-home', routerLink: '/' };
    }

  onChangeVisibleMenu(event: boolean) {
    this.isVisibleMenu = event;
  }

  @HostListener('window:resize')
  checkIfMobile(): void {
    const mobile = window.matchMedia('(max-width: 573px)').matches;

    if (this.isMobile !== mobile) {
      this.isMobile = mobile;
      this.isVisibleMenu = !mobile;
    }
  }
}
