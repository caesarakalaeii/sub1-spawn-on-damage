{
  description = "sub1-spawn-on-damage -- Subnautica BepInEx 5 mod: spawn a pool creature when the player takes damage. `dotnet test` and `dotnet build -c Release` are the gates.";

  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";

  outputs = { self, nixpkgs, ... }:
    let
      systems = [ "x86_64-linux" ];
      forAllSystems = f: nixpkgs.lib.genAttrs systems (system: f nixpkgs.legacyPackages.${system});
    in
    {
      devShells = forAllSystems (pkgs: {
        default = pkgs.mkShell {
          packages = with pkgs; [
            dotnetCorePackages.sdk_10_0
            git
          ];

          # The SDK is read-only in the store; without this, restore fails
          # trying to write its first-run marker. Same as the sibling repo.
          DOTNET_CLI_TELEMETRY_OPTOUT = "1";
          DOTNET_NOLOGO = "1";
          DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1";

          shellHook = ''
            echo "sub1-spawn-on-damage -- dotnet $(dotnet --version 2>/dev/null || echo '?')"
            echo "gates: nix develop -c dotnet test / nix develop -c dotnet build SpawnOnDamage.slnx -c Release"
          '';
        };
      });
    };
}
