test:
	dotnet publish && \
	sudo cp -r bin/Release/* /home/cs2server/serverfiles/game/csgo/addons/counterstrikesharp && \
	sudo chown -R cs2server:cs2server /home/cs2server/serverfiles/game/csgo/addons/counterstrikesharp