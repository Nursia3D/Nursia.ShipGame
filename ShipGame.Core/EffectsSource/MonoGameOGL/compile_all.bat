mgfxc "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\AnimSprite.fx" "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\MonoGameOGL\bin\AnimSprite.efb" /Profile:OpenGL
@if %errorlevel% neq 0 exit /b %errorlevel%

mgfxc "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\Blur.fx" "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\MonoGameOGL\bin\Blur.efb" /Profile:OpenGL
@if %errorlevel% neq 0 exit /b %errorlevel%

mgfxc "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\NormalMapping.fx" "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\MonoGameOGL\bin\NormalMapping.efb" /Profile:OpenGL
@if %errorlevel% neq 0 exit /b %errorlevel%

mgfxc "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\Particle.fx" "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\MonoGameOGL\bin\Particle.efb" /Profile:OpenGL
@if %errorlevel% neq 0 exit /b %errorlevel%
