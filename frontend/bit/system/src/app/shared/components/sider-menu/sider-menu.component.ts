import { Component, Input, OnInit } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { PanelMenu } from 'primeng/panelmenu';
import { BadgeModule } from 'primeng/badge';
import { Ripple } from 'primeng/ripple';
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-sider-menu',
  standalone: true,
  imports: [PanelMenu, BadgeModule, Ripple, CommonModule],
  templateUrl: './sider-menu.component.html',
  styleUrls: ['./sider-menu.component.scss'],
})
export class SiderMenuComponent {
  @Input() collapsed: boolean = false;

  theme: 'dark' | 'light' = 'light';

  menu: any = [];

  constructor() {}

  // ngOnInit() {
  //   // this.themeService.theme$.subscribe(theme => {
  //   //   this.theme = theme;
  //   // });
  //   this.getMenu();
  // }

  getMenu() {
    // const _menu = localStorage.getItem('BIT.user');

    // if (_menu) {
    //   const menuData = JSON.parse(_menu);
    //   const menus = menuData.claims.menu;
    //   this.menu = menus;
    // } else {
    // }

    this.menu = [
      {
        name: 'Financeiro',
        icon: 'dollar',
        submenus: [
          {
            name: 'Receitas',
            route: '/financial/income',
          },
          {
            name: 'Despesas',
            route: '',
          },
           {
            name: 'Fluxo de Caixa',
            route: '',
          },
           {
            name: 'Planejamento',
            route: '',
          },
        ],
      },
      {
        name: 'Pedidos',
        icon: 'shop',
        submenus: [
          {
            name: 'Compras',
            route: '',
          },
          {
            name: 'Vendas',
            route: '',
          },
        ],
      },
      {
        name: 'Estoque',
        icon: 'dropbox',
        submenus: [
          {
            name: 'Produtos',
            route: '',
          },
          {
            name: 'Movimentações',
            route: '',
          },
        ],
      },
    ];
  }

  items: MenuItem[] = [];

    ngOnInit() {
        this.items = [
            {
                label: 'Home',
                icon: 'fa fa-home',
                badge: '5',
                items: [
                    {
                        label: 'Compose',
                        icon: 'pi pi-file-edit',
                        shortcut: '⌘+N'
                    },
                    {
                        label: 'Inbox',
                        icon: 'pi pi-inbox',
                        badge: '5'
                    },
                    {
                        label: 'Sent',
                        icon: 'pi pi-send',
                        shortcut: '⌘+S'
                    },
                    {
                        label: 'Trash',
                        icon: 'pi pi-trash',
                        shortcut: '⌘+T'
                    }
                ]
            },
            {
                label: 'Reports',
                icon: 'pi pi-chart-bar',
                shortcut: '⌘+R',
                items: [
                    {
                        label: 'Sales',
                        icon: 'pi pi-chart-line',
                        badge: '3'
                    },
                    {
                        label: 'Products',
                        icon: 'pi pi-list',
                        badge: '6'
                    }
                ]
            },
            {
                label: 'Profile',
                icon: 'pi pi-user',
                shortcut: '⌘+W',
                items: [
                    {
                        label: 'Settings',
                        icon: 'pi pi-cog',
                        shortcut: '⌘+O'
                    },
                    {
                        label: 'Privacy',
                        icon: 'pi pi-shield',
                        shortcut: '⌘+P'
                    }
                ]
            }
        ];
    }

    toggleAll() {
        const expanded = !this.areAllItemsExpanded();
        this.items = this.toggleAllRecursive(this.items, expanded);
    }

    private toggleAllRecursive(items: MenuItem[], expanded: boolean): MenuItem[] {
        return items.map((menuItem) => {
            menuItem.expanded = expanded;
            if (menuItem.items) {
                menuItem.items = this.toggleAllRecursive(menuItem.items, expanded);
            }
            return menuItem;
        });
    }

    private areAllItemsExpanded(): boolean {
        return this.items.every((menuItem) => menuItem.expanded);
    }
}
