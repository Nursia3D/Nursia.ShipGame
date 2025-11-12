mgfxc "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\AnimSprite.fx" "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\MonoGameDX11\bin\AnimSprite.efb" /Profile:DirectX_11
@if %errorlevel% neq 0 exit /b %errorlevel%

mgfxc "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\Blur.fx" "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\MonoGameDX11\bin\Blur.efb" /Profile:DirectX_11
@if %errorlevel% neq 0 exit /b %errorlevel%

mgfxc "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\NormalMapping.fx" "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\MonoGameDX11\bin\NormalMapping.efb" /Profile:DirectX_11
@if %errorlevel% neq 0 exit /b %errorlevel%

mgfxc "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\Particle.fx" "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\MonoGameDX11\bin\Particle.efb" /Profile:DirectX_11
@if %errorlevel% neq 0 exit /b %errorlevel%
