IMAGE_NAME := rideit
PORT := 8080
RESOURCE_GROUP := rideit-rg
LOCATION := westeurope
ACR_NAME := clusterleansandbox

SOURCES := $(shell find rideit -name '*.cs' -o -name '*.csproj') Dockerfile

.PHONY: build run stop test benchmark clean infra infra-remove push login func

build: .image-built

.image-built: $(SOURCES)
	docker build -t $(IMAGE_NAME) .
	@touch .image-built

run: build
	docker run -d --rm --name $(IMAGE_NAME) -p $(PORT):8080 $(IMAGE_NAME)

stop:
	docker stop $(IMAGE_NAME)

test:
	dotnet test

benchmark:
	drill --benchmark drill.yml --stats --quiet

func:
	cd rideit.Functions && func start

clean:
	docker rmi $(IMAGE_NAME)
	@rm -f .image-built

# --- Azure Deployment ---

infra: infra-rg infra-acr build push infra-deploy

infra-rg:
	az group create --name $(RESOURCE_GROUP) --location $(LOCATION)

infra-acr:
	az deployment group create \
		--resource-group $(RESOURCE_GROUP) \
		--template-file infra/acr.bicep \
		--parameters appName=$(ACR_NAME)

login:
	az acr login --name $(ACR_NAME)

push: login
	docker tag $(IMAGE_NAME) $(ACR_NAME).azurecr.io/$(IMAGE_NAME):latest
	docker push $(ACR_NAME).azurecr.io/$(IMAGE_NAME):latest

infra-deploy:
	$(eval ACR_PASSWORD := $(shell az acr credential show --name $(ACR_NAME) --query "passwords[0].value" -o tsv))
	$(eval ACR_USERNAME := $(shell az acr credential show --name $(ACR_NAME) --query "username" -o tsv))
	az deployment group create \
		--resource-group $(RESOURCE_GROUP) \
		--template-file infra/main.bicep \
		--parameters \
			appName=$(IMAGE_NAME) \
			containerImage=$(ACR_NAME).azurecr.io/$(IMAGE_NAME):latest \
			registryServer=$(ACR_NAME).azurecr.io \
			registryUsername=$(ACR_USERNAME) \
			registryPassword=$(ACR_PASSWORD)

infra-remove:
	az group delete --name $(RESOURCE_GROUP) --yes
