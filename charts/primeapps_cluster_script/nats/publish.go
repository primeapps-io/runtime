package main

import (
	"log"

	"github.com/nats-io/go-nats"
)


func main() {

	nc, err := nats.Connect("nats://primeapps:pr%21m%E2%82%ACAppsi0@35.240.38.31:4222")
	if err != nil {
		log.Fatal(err)
	}
	defer nc.Close()


	nc.Publish("test", []byte("Hello World"))
	nc.Flush()

	if err := nc.LastError(); err != nil {
		log.Fatal(err)
	} else {
		log.Printf("Published [%s] : '%s'\n", "test", "Hello World")
	}
}
