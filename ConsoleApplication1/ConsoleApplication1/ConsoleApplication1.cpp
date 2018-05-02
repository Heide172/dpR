// ConsoleApplication1.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "asyncServ.h"
#include <iostream> 

int _tmain(int argc, _TCHAR* argv[])
{
	setlocale(LC_ALL, "Russian");
	udpServ srv;
	asyncServ serv(&srv); 
	int x;
	while (1)
	{
		std::cin >> x;
		switch (x)
		{
		default:
			break;
		case 1:
			serv.AsyncStart();
			break;
		case 2:
			serv.Pause();
			break;
		case 3:
			serv.Continue();
			break;
		case 4:
			serv.Stop();
			break;
		}
	}
	
	return 0;
}

