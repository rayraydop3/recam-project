import { NavLink, Outlet, useNavigate } from 'react-router-dom'

import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'

import { useAuthStore } from '@/stores/authStore'

// ─── Nav Items ────────────────────────────────────────────────────────────────

const NAV_ITEMS = [
  { label: 'Listings', path: '/admin/listings' },
]

// ─── Component ────────────────────────────────────────────────────────────────

export default function AdminLayout() {
  const navigate = useNavigate()
  const { user, clearAuth } = useAuthStore()

  const handleLogout = () => {
    clearAuth()
    navigate('/login')
  }

  const initials = user?.email?.slice(0, 2).toUpperCase() ?? 'AD'

  return (
    <div className="min-h-screen flex flex-col">

      {/* Top Navbar */}
      <header className="border-b bg-background sticky top-0 z-10">
        <div className="flex items-center justify-between px-6 h-14">

          {/* Logo */}
          <div className="flex items-center gap-8">
            <span className="font-bold text-lg tracking-tight">Recam</span>

            {/* Nav Links */}
            <nav className="flex items-center gap-1">
              {NAV_ITEMS.map((item) => (
                <NavLink
                  key={item.path}
                  to={item.path}
                  className={({ isActive }) =>
                    `px-3 py-1.5 rounded-md text-sm transition-colors ${
                      isActive
                        ? 'bg-muted font-medium text-foreground'
                        : 'text-muted-foreground hover:text-foreground hover:bg-muted'
                    }`
                  }
                >
                  {item.label}
                </NavLink>
              ))}
            </nav>
          </div>

          {/* User Menu */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="relative h-8 w-8 rounded-full">
                <Avatar className="h-8 w-8">
                  <AvatarFallback className="text-xs">{initials}</AvatarFallback>
                </Avatar>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-48">
              <div className="px-2 py-1.5 text-sm text-muted-foreground truncate">
                {user?.email}
              </div>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={handleLogout} className="text-destructive">
                Log out
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>

        </div>
      </header>

      {/* Page Content */}
      <main className="flex-1">
        <Outlet />
      </main>

    </div>
  )
}
