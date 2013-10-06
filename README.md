GivenWhenUnity
==============

Behavior driven development tool for Unity3D

Real Life Example
=================

    using UnityEngine;
    using System.Collections;
    using Require;
    using Shouldly;
    using System;
    
    public class DealsPointsInCollisionTest : TestBehaviour
    {
        DealsPointsInCollision it;
        GameObject butthead;
    
        public override void Spec()
        {
            Given("it deals 3 'damage' in collisions")
                .And("a butthead with 5 hp is nearby")
                .When("it collides with the butthead")
                .ThenWithin("3 frames", "the butthead should have 2 hp");
    
            Given("it deals 1 'damage' in collisions")
                .And("it is destroyed after dealing")
                .And("a butthead with 5 hp is nearby")
                .When("it collides with the butthead")
                .ThenWithin("3 frames", "it should be destroyed");
    
            Given("it deals 1 'damage' in collisions")
                .And("it is deactivated after dealing")
                .And("a butthead with 5 hp is nearby")
                .When("it collides with the butthead")
                .ThenWithin("3 frames", "it should be deactivated");
        }
    
        public void ItDeals____InCollisions(float amount, string source)
        {
            it = new GameObject().transform.Require<DealsPointsInCollision>();
            it.transform.position = transform.position;
            it.transform.Require<BoxCollider>().isTrigger = true;
            it.transform.Require<Rigidbody>().isKinematic = true;
            it.source = source;
            it.amount = amount;
        }
    
        public void ItIsDestroyedAfterDealing()
        {
            it.afterDealing = DealsPointsInCollision.AfterDealing.Destroy;
        }
    
        public void ItIsDeactivatedAfterDealing()
        {
            it.afterDealing = DealsPointsInCollision.AfterDealing.Deactivate;
        }
    
        public void AButtheadWith__HpIsNearby(float hp)
        {
            butthead = new GameObject();
            butthead.transform.Require<BoxCollider>().isTrigger = true;
            butthead.transform.Require<Rigidbody>().isKinematic = true;
            butthead.transform.position = transform.position + Vector3.left * 5;
    
            HasPoints buttheadPoints = butthead.transform.Require<HasPoints>();
            buttheadPoints.Set("hp", hp);
            buttheadPoints.SetModifier("hp", "damage", -1);
        }
    
        public void ItCollidesWithTheButthead()
        {
            butthead.transform.position = transform.position;
        }
    
        public void TheButtheadShouldHave__Hp(float hp)
        {
            butthead.transform.Require<HasPoints>().Get("hp").ShouldBe(hp);
        }
    
        public void ItShouldBeDestroyed()
        {
            if (it != null)
            {
                throw new Exception("Expected it to be destroyed but it wasn't");
            }
        }
    
        public void ItShouldBeDeactivated()
        {
            it.gameObject.activeSelf.ShouldBe(false);
        }
    }

In Progress
===========

__.Because()__

    Given("i write a test")
        .When("it is displayed")
        .Then("it should tell me why i even bothered to write that test")
        .Because("it's not always clear after a few beers");
