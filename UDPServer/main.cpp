//////////// Server.cpp /////////////

#include<stdio.h>
#include<stdlib.h>
#include<string.h>
#include<sys/types.h>
#include<sys/socket.h>
#include<arpa/inet.h>
#include<unistd.h>
#include<iostream>

using namespace std;


#define BUFSIZE 512


int main()
{

    // socket() 생성
    int sock = socket(AF_INET, SOCK_DGRAM, 0); //(주소 체계, 소켓 타입, 프로토콜)
    int retval;


    // bind() 소켓에 주소 할당
    sockaddr_in serveraddr;
    bzero(&serveraddr, sizeof(serveraddr)); //0으로 초기화
    serveraddr.sin_family = AF_INET;
    serveraddr.sin_port = htons(23456);
    serveraddr.sin_addr.s_addr = htonl(INADDR_ANY);
    retval = bind(sock, (sockaddr*)&serveraddr, sizeof(serveraddr));


    // 데이터 통신에 사용할 변수
    sockaddr_in clientaddr;
    int addrlen;
    char buf[BUFSIZE + 1];


    // 클라이언트와 데이터 통신
    while (1)
    {


        // 데이터 받기
        addrlen = sizeof(clientaddr);
        retval = recvfrom(sock, buf, BUFSIZE, 0, (sockaddr*)&clientaddr,

            (socklen_t*)&addrlen);


        //받은 데이터 출력
        buf[retval] = '\0';
        cout << "[UDP/" << inet_ntoa(clientaddr.sin_addr) << ":"

            << ntohs(clientaddr.sin_port) << "] " << buf << endl;


        //데이터 보내기
        retval = sendto(sock, buf, retval, 0, (sockaddr*)&clientaddr, sizeof(clientaddr));

        //(소켓,보낼 데이터 주소,데이터 크기,0,목적지 주소, 소켓 주소 구조체 크기)

    }


    // 클라이언트 소켓 연결 종료
    close(sock);

    return 0;
}
