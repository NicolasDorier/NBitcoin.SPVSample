# NBitcoin.SPVSample
SPV wallet sample created with NBitcoin and WPF

This is a simple program which show how to create multi sig (or not) SPV wallets with NBitcoin.
It works on TestNet, please don't use this program in production, I do not handle any error condtions and store private keys in clear.

In a nutshell, a SPV app is using several part of NBitcoin :

* The NodesGroup which will keep connection to a group of nodes open
* The Tracker which will keep track of all outpoints and addresses that your wallets are tracking.
* The Chain which is all the block headers in the current chain.
* The AddressManager which is a set of discovered peers, so peer discovery is faster
* The Wallet is tracking the list of known scriptPubKey and the root HD pub keys so it can generate new addresses on demand.

All these data structures are "attached" to nodes discovered by NodesGroup by using respectively TrackerBehavior, ChainBehavior and AddressManagerBehavior.

* TrackerBehavior set the bloom filter to remote peers, scan the merkle blocks and push that information in the Tracker.
* ChainBehavior listens new incoming blocks from a peer so it can keep the Chain in sync.
* AddressManagerBehavior listens getaddr/addr messages and registers them for later use during the next peer discovery process.

This program periodically save the Chain, AddressManager, Wallet and Tracker.

The implementation of TrackerBehavior is privacy friendly. All the wallets are sharing the same bloom filter, the bloom filter is preloaded with 1000 keys per wallet and never updated.
Every 10 minutes, it disconnects from peers and reconnect to new ones with the same filter. I followed [this paper](http://eprint.iacr.org/2014/763.pdf), and improved on it.

This paper was oblivous to the fact that filters need to be reloaded periodically since at every false positive, the filter matches more objects.
But if the filter is renewed on the same peer, then by doing a differential of the two filters, a malicious peer can find out which coins belongs to you.

If the bloom filter need to be reloaded (for generating a new batch of 1000 keys), then the connections to the current peers are purged, and new nodes are found.
