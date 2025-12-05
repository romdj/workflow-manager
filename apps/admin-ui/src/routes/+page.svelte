<script lang="ts">
  // Page logic will go here
</script>

<div class="fade-in">
  <div class="hero min-h-[60vh] bg-base-200 rounded-lg">
    <div class="hero-content text-center">
      <div class="max-w-md">
        <h1 class="text-5xl font-bold text-primary">Workflow Manager</h1>
        <p class="py-6 text-base-content/70">
          Welcome to the Workflow Manager administration interface. Manage your workflows, monitor executions, and
          configure settings.
        </p>
        <div class="flex gap-4 justify-center">
          <a href="/workflows" class="btn btn-primary">View Workflows</a>
          <a href="/dashboard" class="btn btn-outline btn-secondary">Dashboard</a>
        </div>
      </div>
    </div>
  </div>

  <!-- Quick Stats -->
  <div class="stats stats-vertical lg:stats-horizontal shadow mt-8 w-full">
    <div class="stat">
      <div class="stat-figure text-primary">
        <svg
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          class="inline-block h-8 w-8 stroke-current"
        >
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
        </svg>
      </div>
      <div class="stat-title">Active Workflows</div>
      <div class="stat-value text-primary">0</div>
      <div class="stat-desc">Ready to execute</div>
    </div>

    <div class="stat">
      <div class="stat-figure text-secondary">
        <svg
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          class="inline-block h-8 w-8 stroke-current"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4"
          />
        </svg>
      </div>
      <div class="stat-title">Executions Today</div>
      <div class="stat-value text-secondary">0</div>
      <div class="stat-desc">Last 24 hours</div>
    </div>

    <div class="stat">
      <div class="stat-figure text-accent">
        <svg
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          class="inline-block h-8 w-8 stroke-current"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M9 12l2 2 4-4M7.835 4.697a3.42 3.42 0 001.946-.806 3.42 3.42 0 014.438 0 3.42 3.42 0 001.946.806 3.42 3.42 0 013.138 3.138 3.42 3.42 0 00.806 1.946 3.42 3.42 0 010 4.438 3.42 3.42 0 00-.806 1.946 3.42 3.42 0 01-3.138 3.138 3.42 3.42 0 00-1.946.806 3.42 3.42 0 01-4.438 0 3.42 3.42 0 00-1.946-.806 3.42 3.42 0 01-3.138-3.138 3.42 3.42 0 00-.806-1.946 3.42 3.42 0 010-4.438 3.42 3.42 0 00.806-1.946 3.42 3.42 0 013.138-3.138z"
          />
        </svg>
      </div>
      <div class="stat-title">Success Rate</div>
      <div class="stat-value text-accent">100%</div>
      <div class="stat-desc">All time</div>
    </div>
  </div>
</div>
