fxc "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\AnimSprite.fx" /Fo "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\FNA\bin\AnimSprite.efb" /T:fx_2_0
@if %errorlevel% neq 0 exit /b %errorlevel%

fxc "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\Blur.fx" /Fo "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\FNA\bin\Blur.efb" /T:fx_2_0
@if %errorlevel% neq 0 exit /b %errorlevel%

fxc "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\NormalMapping.fx" /Fo "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\FNA\bin\NormalMapping.efb" /T:fx_2_0
@if %errorlevel% neq 0 exit /b %errorlevel%

fxc "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\Particle.fx" /Fo "D:\Projects\ShipGameVariant\ShipGame.Core\EffectsSource\FNA\bin\Particle.efb" /T:fx_2_0
@if %errorlevel% neq 0 exit /b %errorlevel%
