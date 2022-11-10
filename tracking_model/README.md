# Python 파일 설치 and 구동방법

## 설치방법
1. 아나콘다 프롬프트 설치<br>
https://www.anaconda.com

2. conda create -n ID python=3.8<br>
ID에 원하는 이름 기입

3. activate ID 입력

4. OpenCV, mediapipe 설치<br>
   
    pip install mediapipe<br>
    pip install opencv-python 
   

## 실행방법
1. anaconda prompt 실행
2. activate ID 입력
3. cd 명령어로 tracking_model폴더로 이동
4. python main.py --connect --debug --port 9999 실행
