<script lang="ts">
  import '../app.css';
</script>

<div class="min-h-screen bg-base-100">
  <!-- Header -->
  <header class="navbar bg-base-100 shadow-sm border-b border-base-200">
    <div class="container mx-auto">
      <div class="flex-1">
        <a href="/" class="btn btn-ghost normal-case text-xl text-primary font-bold"> ⚙️ Workflow Manager </a>
        <div class="badge badge-secondary ml-2">Admin</div>
      </div>
      <div class="flex-none">
        <ul class="menu menu-horizontal px-1">
          <li><a href="/workflows">Workflows</a></li>
          <li><a href="/dashboard">Dashboard</a></li>
        </ul>
      </div>
    </div>
  </header>

  <!-- Main content -->
  <main class="container mx-auto px-4 py-6">
    <slot />
  </main>

  <!-- Footer -->
  <footer class="footer footer-center p-4 bg-base-200 text-base-content">
    <div>
      <p class="text-sm">
        Workflow Manager Admin Dashboard • Built with
        <a href="https://svelte.dev" class="link link-primary">SvelteKit</a>
      </p>
    </div>
  </footer>
</div>

<style>
  .container {
    max-width: 1200px;
  }
</style>
