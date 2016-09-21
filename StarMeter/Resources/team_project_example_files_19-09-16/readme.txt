Industrial Team Project Example Files
-------------------------------------

These files demonstrate the following errors:

Test 1: This shows an invalid sequence number.

Test 2: This shows a packet timing out at a router.  The link 1 recording shows
        the traffic before it enters the router, with link 3 showing the traffic
        once it has been routed.

Test 3: This shows a babbling idiot which repeatedly sends the same packet until
        it is detected.

Test 4: This shows an invalid CRC in an RMAP packet.  Link 1 shows the RMAP
        commands and link 2 shows the replies.  To detect the cause of this
        error you would need to know the format of RMAP packets and check the
		CRCs of each one.

Test 5: This shows a packet which is longer than expected.  Again this is an
        RMAP packet with link 1 showing the commands and link 2 the replies.  As
        the packets are RMAP packets, the error could be detected by checking
        the CRC or determining the expected length from the RMAP fields.
        Alternatively the error could be found by looking at the previous
        packets.  

Test 6: This test shows a device which is periodically reading the status of two
        other devices using RMAP read operations every second.  Every 10 seconds
        there is a parity error encountered on the link, which then results on a
        disconnect on the link.  The packet that was being transmitted on the
        link is terminated by an EEP by the receiving router when it is
        forwarded on.  The arrangement of this network is as follows:
         - Initiating device (Device 1) connected to port 2 of a router (Router A).
         - Port 1 of Router A is connected to port 3 of another router (Router B).
         - Ports 1 and 2 of Router B are connected to the two devices which
           are having their status read (Devices 2 and 3).
        The Recorder is included at the following locations:
         - Ports 1 and 2 are connected between Device 2 and Router B.
         - Ports 3 and 4 are connected between Device 3 and Router B.
         - Ports 5 and 6 are connected between Router B and Router A.
         - Ports 7 and 8 are connected between Router A and Device 1.
        Note that the order in which the devices/routers are specified indicates
		which port of the Recorder they are connected to.

Note that in all tests, the link is disconnected after each error is
encountered.  It can be assumed this will always happen after an error.



Change Log
----------

16-09-16
--------
- Changed structure of files, so that each test has its own directory, and each
  file is named the same for all tests.
- Fixed issue in test 4 with seconds having a value of 60.
- Added Test 6.


13-09-16
--------
Initial version.
