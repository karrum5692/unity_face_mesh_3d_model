set root=C:\Users\HP\miniconda3
call %root%\Scripts\activate.bat %root%

call conda env list
call conda activate mediapipe
call cd ..
call cd VTuber
call python main.py

pause
