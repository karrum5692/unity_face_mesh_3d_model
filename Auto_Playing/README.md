# Anaconda 자동실행 설정

1. [파일 설정](#파일-설정)
2. [Batch 파일 설정](#Batch-파일-설정)

-----------
## 파일 설정

1. Unity 빈 empty 생성 후 PlayingLauncher.cs할당

2. CS File 열어서 코드 수정

    string pythonPath = @"bat파일 경로";
    
    ex) string pythonPath = @"C:\Users\HP\Desktop\launcher.bat";
      
## Batch 파일 설정
    set root=C:\Users\HP\miniconda3 
    //miniconda경로 설정(유저마다 다름)
    call %root%\Scripts\activate.bat %root%

    call conda env list
    call conda activate 이름
    // 개인이 설정한 이름
    call cd ..
    // vruber 폴더를 프로젝트와 같은 공간에 두었을 경우 
    call cd VTuber
    call python main.py

    pause
