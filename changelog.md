## 8/4/2025

* FrontEnd: Update the Auth Service to rely on the Auth Guard for a list of gaurded paths. This creates a single list of guarded paths instead of trying to maintain two identical lists. The Auth Guard (via the router) is now the single source of truth
* FrontEnd: Set up a global store for the project using NgRX
* FrontEnd: Create a global store for supported currencies
* FrontEnd: Load supported currencies from the back end on app startup. (Currently only Ethereum is supported). 